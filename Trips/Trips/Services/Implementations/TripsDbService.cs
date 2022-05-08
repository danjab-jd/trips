using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Trips.DTO;
using Trips.Entities;
using Trips.Services.Interfaces;

namespace Trips.Services.Implementations
{
    public class TripsDbService : ITripsDbService
    {
        private readonly TripsContext _context;

        public TripsDbService(TripsContext context)
        {
            _context = context;
        }

        public async Task<IList<TripDTO>> GetTripsListAsync()
        {
            return await _context.Trips
                .Include(x => x.ClientTrips).ThenInclude(x => x.IdClientNavigation)
                .Include(x => x.CountryTrips).ThenInclude(x => x.IdCountryNavigation)
                .Select(x => new TripDTO
                {
                    Name = x.Name,
                    Description = x.Description,
                    DateFrom = x.DateFrom,
                    DateTo = x.DateTo,
                    MaxPeople = x.MaxPeople,
                    Countries = x.CountryTrips.Select(y => new CountryDTO
                    {
                        Name = y.IdCountryNavigation.Name
                    }).ToList(),
                    Clients = x.ClientTrips.Select(y => new ClientDTO
                    {
                        FirstName = y.IdClientNavigation.FirstName,
                        LastName = y.IdClientNavigation.LastName
                    }).ToList()
                }).OrderByDescending(x => x.DateFrom).ToListAsync();
        }

        public async Task<MethodResultDTO> AddClientToTripAsync(ClientToTripDTO clientToTripDTO)
        {
            try
            {
                // Wołanie SaveChanges przetwarza wszystkie śledzone operacje w ramach 1 transakcji.
                // Na potrzeby zajęć w zupełności wystarczy wołać tylko SaveChanges

                // Natomiast zastosowanie przeze mnie ręcznego tworzenia transakcji jest zaprezentowane w celach informacyjnych jak to się robi przy EF
                // Link: https://docs.microsoft.com/en-us/ef/core/saving/transactions

                IDbContextTransaction transaction = await _context.Database.BeginTransactionAsync();

                Trip tripFromDb = await GetTripFromDb(clientToTripDTO.IdTrip, clientToTripDTO.TripName);

                if (tripFromDb == null)
                {
                    await transaction.RollbackAsync();

                    return new MethodResultDTO
                    {
                        HttpStatusCode = HttpStatusCode.NotFound,
                        Message = "Trip does not exist in the db"
                    };
                }


                Client clientFromDb = await GetClientByPeselAsync(clientToTripDTO.Pesel);

                if (clientFromDb == null)
                {
                    clientFromDb = await AddClientAsync(new AddClientDTO
                    {
                        FirstName = clientToTripDTO.FirstName,
                        LastName = clientToTripDTO.LastName,
                        Email = clientToTripDTO.Email,
                        Pesel = clientToTripDTO.Pesel,
                        Telephone = clientToTripDTO.Telephone
                    });
                }
                else
                {
                    if (await CheckIfUserAlreadyAssignedToTrip(clientFromDb.IdClient, tripFromDb.IdTrip))
                    {
                        await transaction.RollbackAsync();

                        return new MethodResultDTO
                        {
                            HttpStatusCode = HttpStatusCode.BadRequest,
                            Message = "Client already assigned to given trip"
                        };
                    }
                }

                AssignTripToClient(clientFromDb, tripFromDb, clientToTripDTO.PaymentDate);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return new MethodResultDTO
                {
                    HttpStatusCode = HttpStatusCode.OK
                };

            }
            catch (Exception ex)
            {
                return new MethodResultDTO
                {
                    HttpStatusCode = HttpStatusCode.InternalServerError,
                    Message = ex.Message
                };
            }
        }

        private void AssignTripToClient(Client client, Trip trip, DateTime? paymentDate)
        {
            client.ClientTrips.Add(new ClientTrip
            {
                IdTripNavigation = trip,
                PaymentDate = paymentDate,
                RegisteredAt = DateTime.Now
            });
        }

        private async Task<Trip> GetTripFromDb(int idTrip, string tripName)
        {
            return await _context.Trips.SingleOrDefaultAsync(x => x.IdTrip == idTrip && x.Name == tripName);
        }

        private async Task<Client> GetClientByPeselAsync(string pesel)
        {
            return await _context.Clients.SingleOrDefaultAsync(x => x.Pesel == pesel);
        }

        private async Task<Client> AddClientAsync(AddClientDTO clientToAdd)
        {
            int newId = !await _context.Clients.AnyAsync() ? 1 : await _context.Clients.MaxAsync(x => x.IdClient) + 1;

            Client newClient = new Client
            {
                IdClient = newId,
                FirstName = clientToAdd.FirstName,
                LastName = clientToAdd.LastName,
                Pesel = clientToAdd.Pesel,
                Email = clientToAdd.Email,
                Telephone = clientToAdd.Telephone
            };

            await _context.Clients.AddAsync(newClient);

            return newClient;
        }

        private async Task<bool> CheckIfUserAlreadyAssignedToTrip(int idClient, int idTrip)
        {
            return await _context.ClientTrips.AnyAsync(x => x.IdClient == idClient && x.IdTrip == idTrip);
        }

    }

}
