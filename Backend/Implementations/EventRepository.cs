﻿//using Backend.Database.Access;
using Backend.Model;
using System.Collections.Generic;
using Backend.Inferfaces;
using System.Linq;
using System;
using Shared.Model;
using Shared.Global;
using FileOperations.Implementations;
using FileOperations.Interfaces;
//using Microsoft.SqlServer.Server;

namespace Backend.Implementations
{
    /// <summary>
    /// Class for the Event Repository
    /// </summary>
    public class EventRepository : IEventRepository
    {
        //private readonly DataLayer _dataAccess;

        private readonly List<SavedEvent> SavedEvents;
        private readonly IFileOperations<SavedEvent> io;

        /// <summary>
        /// Construtor for the EventRepository
        /// </summary>
        public EventRepository()
        {
            SavedEvents = new List<SavedEvent>();
            io = new EventIO();
            //_dataAccess = new DataLayer();

            // Tables needed from database (Find a way to use model objects to format data)
            // Get rid of Bookmarks
            // Might remove Event Detail Repo
        }

        /// <summary>
        /// Implements <see cref="IEventRepository.AddEvent(SavedEvent)" />
        /// </summary>
        public bool AddEvent(SavedEvent aEvent)
        {
            try
            {
                SavedEvents.Add(aEvent);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Implements <see cref="IEventRepository.UpdateEvent(SavedEvent)" />
        /// </summary>
        public bool UpdateEvent(SavedEvent @event)
        {
            try
            {
                SavedEvent existingEvent = SavedEvents.SingleOrDefault(x => x.Id == @event.Id);

                if (existingEvent == null)
                    throw new Exception("Event not found.");

                existingEvent.ActivationDate = @event.ActivationDate;
                existingEvent.ActivationTime = @event.ActivationTime;
                existingEvent.Comment = @event.Comment;
                existingEvent.Completed = @event.Completed;
                existingEvent.DeactivationDate = @event.DeactivationDate;
                existingEvent.DeactivationTime = @event.DeactivationTime;
                existingEvent.Title = @event.Title;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Implements <see cref="IEventRepository.DeleteEvent(string)" />
        /// </summary>
        public bool DeleteEvent(string id)
        {
            try
            {
                SavedEvent existingEvent = SavedEvents.SingleOrDefault(x => x.Id == id);

                if (existingEvent == null)
                    throw new Exception("Event not found.");

                return SavedEvents.Remove(existingEvent);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Implements <see cref="IEventRepository.GetEvent(string)" />
        /// </summary>
        public SavedEvent GetEvent(string id)
        {
            //SavedEvent theEvent = null;
            //var events = _dataAccess.GetDataFromTable("Some query");

            if (string.IsNullOrEmpty(id))
                return null;

            SavedEvent @event = SavedEvents.SingleOrDefault(x => x.Id == id);

            return @event;
        }

        /// <summary>
        /// Implements <see cref="IEventRepository.GetEvents(Date, Date)" />
        /// </summary>
        public IEnumerable<SavedEvent> GetEvents(Date start, Date end)
        {
            return
                SavedEvents.Where(x =>
                                TimeAndDateUtility.IsWithinRange(start, x.ActivationDate, end)
                                || TimeAndDateUtility.IsWithinRange(start, x.DeactivationDate, end))
                           .ToList()
                           .AsReadOnly();
        }

        /// <summary>
        /// Implements <see cref="IEventRepository.GetEvents(string)" />
        /// </summary>
        public IEnumerable<SavedEvent> GetEvents(string searchTerm)
        {
            string loweredString = searchTerm.ToLower();
            return SavedEvents.Where(x =>
                                x.Title.ToLower().Contains(loweredString)
                                || x.Comment.ToLower().Contains(loweredString))
                              .ToList()
                              .AsReadOnly();
        }

        /// <summary>
        /// Implements <see cref="IEventRepository.GetEvents(Date)" />
        /// </summary>
        public IEnumerable<SavedEvent> GetEvents(Date date)
        {
            return
                SavedEvents.Where(x => TimeAndDateUtility.IsWithinRange(x.ActivationDate, date, x.DeactivationDate))
                           .ToList()
                           .AsReadOnly();
        }

        /// <summary>
        /// Implements <see cref="IEventRepository.GetEvents()" />
        /// </summary>
        public IEnumerable<SavedEvent> GetEvents()
        {
            return SavedEvents.AsReadOnly();
        }

        /// <summary>
        /// Implements <see cref="IEventRepository.LoadEvents(bool)" />
        /// </summary>
        public bool LoadEvents(bool overwrite = false)
        {
            if (overwrite)
            {
                SavedEvents.AddRange(io.Load());
            }
            else
            {
                List<SavedEvent> messages = io.Load();
                List<SavedEvent> filtered = messages.Where(x => !SavedEvents.Any(y => y.Id == x.Id)).ToList();
                SavedEvents.AddRange(filtered);
            }

            return true;
        }

        /// <summary>
        /// Implements <see cref="IEventRepository.LoadEventsExternal(string, bool)" />
        /// </summary>
        public bool LoadEventsExternal(string path, bool overwrite = false)
        {
            if (overwrite)
            {
                SavedEvents.AddRange(io.Load(path));
            }
            else
            {
                List<SavedEvent> messages = io.Load(path);
                List<SavedEvent> filtered = messages.Where(x => !SavedEvents.Any(y => y.Id == x.Id)).ToList();
                SavedEvents.AddRange(filtered);
            }

            return true;
        }

        /// <summary>
        /// Implements <see cref="IEventRepository.SaveEvents()" />
        /// </summary>
        public void SaveEvents()
        {
            io.Save(SavedEvents);
        }

        /// <summary>
        /// Implements <see cref="IEventRepository.SaveEventsExternal(string)" />
        /// </summary>
        public void SaveEventsExternal(string path)
        {
            io.Save(SavedEvents, path);
        }
    }
}
