using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;
using UCDArch.Core.DomainModel;

namespace StudentFarm.Models
{
    public class Buyer : DomainObjectWithTypedId<int>
    {
        public virtual string Name { get; set; }

        public virtual IList<BuyerAvailability> Availabilities { get; set; }

        public Buyer()
        {
            Availabilities = new List<BuyerAvailability>();
        }
    }

    public class BuyerMap : ClassMap<Buyer>
    {
        public BuyerMap()
        {
            Id(x => x.Id).GeneratedBy.Identity();
            Map(x => x.Name);
            HasMany(x => x.Availabilities)
                .Cascade.DeleteOrphan()
                .Fetch.Join()
                .Inverse()
                .KeyColumn("BuyerId");
        }
    }
}