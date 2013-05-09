using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;
using UCDArch.Core.DomainModel;

namespace StudentFarm.Models
{
    public class Availability : DomainObjectWithTypedId<int>
    {
        public virtual DateTime DateStart { get; set; }
        public virtual DateTime DateEnd { get; set; }
        public virtual string Comments { get; set; }

        public virtual IList<BuyerAvailability> Buyers { get; set; }
        public virtual IList<Offered> Offered { get; set; }

        public Availability()
        {
            Buyers = new List<BuyerAvailability>();
            Offered = new List<Offered>();
        }
    }

    public class AvailabilityMap : ClassMap<Availability>
    {
        public AvailabilityMap()
        {
            Id(x => x.Id).GeneratedBy.Identity();
            Map(x => x.DateStart);
            Map(x => x.DateEnd);
            Map(x => x.Comments);
            HasMany(x => x.Buyers)
                .Cascade.AllDeleteOrphan()
                .Inverse()
                .Fetch.Join()
                .KeyColumn("AvailabilityId");
            HasMany(x => x.Offered)
                .Cascade.AllDeleteOrphan()
                .Inverse()
                .Fetch.Join()
                .KeyColumn("AvailabilityId");
        }
    }
}