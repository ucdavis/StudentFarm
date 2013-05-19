using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;
using UCDArch.Core.DomainModel;

namespace StudentFarm.Models
{
    public class BuyerAvailability : DomainObjectWithTypedId<int>
    {
        public virtual Buyer Buyer { get; set; }
        public virtual Availability Availability { get; set; }
        public virtual DateTime StartTime { get; set; }
        public virtual DateTime EndTime { get; set; }

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
        }
    }
}