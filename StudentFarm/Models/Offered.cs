using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;
using UCDArch.Core.DomainModel;

namespace StudentFarm.Models
{
    public class Offered : DomainObjectWithTypedId<int>
    {
        public virtual Price CropPrice { get; set; }
        public virtual Availability Availability { get; set; }
        public virtual double Quantity { get; set; }

        public Offered()
        {
        }
    }

    public class OfferedMap : ClassMap<Offered>
    {
        public OfferedMap()
        {
            Id(x => x.Id).GeneratedBy.Identity();
            References(x => x.CropPrice)
                .Not.Nullable()
                .Cascade.SaveUpdate()
                .Column("CropPriceId");
            References(x => x.Availability)
                .Not.Nullable()
                .Cascade.SaveUpdate()
                .Column("AvailabilityId");
            Map(x => x.Quantity);
        }
    }
}