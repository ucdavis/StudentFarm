using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;
using UCDArch.Core.DomainModel;

namespace StudentFarm.Models
{
    public class Invoice : DomainObjectWithTypedId<int>
    {
        public virtual Order Order { get; set; }
        public virtual DateTime Date { get; set; }
        public virtual string Modifications { get; set; }
        public virtual double DeliveryCharge { get; set; }

        public Invoice()
        {
        }
    }

    public class InvoiceMap : ClassMap<Invoice>
    {
        public InvoiceMap()
        {
            Id(x => x.Id).GeneratedBy.Identity();
            References(x => x.Order)
                .Not.Nullable()
                .Cascade.SaveUpdate()
                .Column("OrderId");
            Map(x => x.Date);
            Map(x => x.Modifications);
            Map(x => x.DeliveryCharge);
        }
    }
}