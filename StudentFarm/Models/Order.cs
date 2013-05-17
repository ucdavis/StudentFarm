using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;
using UCDArch.Core.DomainModel;

namespace StudentFarm.Models
{
    public class Order : DomainObjectWithTypedId<int>
    {
        public virtual Buyer Buyer { get; set; }
        public virtual Availability Availability { get; set; }
        public virtual DateTime OrderTime { get; set; }
        public virtual DateTime DeliveryTime { get; set; }
        public virtual string Comments { get; set; }

        public Order()
        {
        }
    }

    public class OrderMap : ClassMap<Order>
    {
        public OrderMap()
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
            Map(x => x.OrderTime);
            Map(x => x.DeliveryTime);
            Map(x => x.Comments).Nullable();
        }
    }
}