using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;
using UCDArch.Core.DomainModel;

namespace StudentFarm.Models
{
    public class Ordered : DomainObjectWithTypedId<int>
    {
        public virtual Order Order { get; set; }
        public virtual Offered Offered { get; set; }
        public virtual float Quantity { get; set; }

        public Ordered()
        {
        }
    }

    public class OrderedMap : ClassMap<Ordered>
    {
        public OrderedMap()
        {
            Id(x => x.Id).GeneratedBy.Identity();
            References(x => x.Order)
                .Not.Nullable()
                .Cascade.SaveUpdate()
                .Column("OrderId");
            References(x => x.Offered)
                .Not.Nullable()
                .Cascade.SaveUpdate()
                .Column("OfferedId");
            Map(x => x.Quantity);
        }
    }
}