using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;
using UCDArch.Core.DomainModel;
using UCDArch.Core.PersistanceSupport;
using UCDArch.Data.NHibernate;

namespace StudentFarm.Models
{
    public class BuyerAvailability : DomainObjectWithTypedId<int>
    {
        public virtual Buyer Buyer { get; set; }
        public virtual Availability Availability { get; set; }
        public virtual DateTime StartTime { get; set; }
        public virtual DateTime EndTime { get; set; }
        public virtual String RelStart { get; set; }
        public virtual String RelEnd { get; set; }

        public BuyerAvailability()
        {
        }

        public virtual int ToMinutes(bool Start)
        {
            int minutes = 0;

            if (Start)
            {
                minutes = StartTime.Hour * 60;
                minutes += StartTime.Minute;
                return minutes;
            }
            else
            {
                minutes = EndTime.Hour * 60;
                minutes += EndTime.Minute;
                return minutes;
            }
        }
    }

    public class BuyerAvailabilityMap : ClassMap<BuyerAvailability>
    {
        public BuyerAvailabilityMap()
        {
            Id(x => x.Id).GeneratedBy.Identity();
            References(x => x.Buyer)
                .Not.Nullable()
                .Cascade.SaveUpdate()
                .Column("BuyerId");
            References(x => x.Availability)
                .Not.Nullable()
                .Cascade.SaveUpdate()
                .Column("AvailabilityId");

            Map(x => x.StartTime);
            Map(x => x.EndTime);
            Map(x => x.RelStart).Nullable();
            Map(x => x.RelEnd).Nullable();
        }
    }

    public interface IBuyerAvailabilityRepository : IRepository<BuyerAvailability>
    {
        BuyerAvailability CreateOrUpdate(int id, Availability avail, Buyer buyer, DateTime start, DateTime end,
            String relstart, String relend);
        BuyerAvailability CreateOrUpdate(int id, Availability avail, Buyer buyer, String start_d,
            String start_t, String end_d, String end_t);
    }

    public class BuyerAvailabilityRepository : Repository<BuyerAvailability>, IBuyerAvailabilityRepository
    {
        public BuyerAvailability CreateOrUpdate(int id, Availability avail, Buyer buyer, DateTime start,
            DateTime end, String relstart, String relend)
        {
            BuyerAvailability buyera = avail.HasBuyer(id);

            if (buyera == null)
            {
                buyera = new BuyerAvailability();
                buyera.Availability = avail;
                buyera.Buyer = buyer;
            }
            buyera.StartTime = start;
            buyera.EndTime = end;
            buyera.RelStart = relstart;
            buyera.RelEnd = relend;

            this.EnsurePersistent(buyera);

            return buyera;
        }

        public BuyerAvailability CreateOrUpdate(int id, Availability avail, Buyer buyer, String start_d,
            String start_t, String end_d, String end_t)
        {
            FuzzyDateParser reldate = new FuzzyDateParser();
            DateTime start = DateTime.Parse(reldate.Parse(start_d, avail.DateStart, true).ToShortDateString() + " " + start_t);
            DateTime end = DateTime.Parse(reldate.Parse(end_d, avail.DateStart, true).ToShortDateString() + " " + end_t);
            return CreateOrUpdate(id, avail, buyer, start, end, start_d, end_d);
        }
    }
}