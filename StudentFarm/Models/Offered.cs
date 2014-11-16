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
    public class Offered : DomainObjectWithTypedId<int>
    {
        public virtual Price CropPrice { get; set; }
        public virtual Availability Availability { get; set; }
        public virtual IList<Ordered> Ordered { get; set; }
        public virtual double Quantity { get; set; }

        public Offered()
        {
        }
    }

    public class OfferedMap : ClassMap<Offered>
    {
        public OfferedMap()
        {
            Table("Offered");
            Id(x => x.Id).GeneratedBy.Identity();
            References(x => x.CropPrice)
                .Not.Nullable()
                .Cascade.None()
                .Column("PriceId"); // The column was originally named CropPriceId, but
                                    // changed it to PriceId due to a bug in UCDArch.
            References(x => x.Availability)
                .Not.Nullable()
                .Cascade.None()
                .Column("AvailabilityId");
            HasMany(x => x.Ordered)
                .Cascade.DeleteOrphan()
                .Inverse();
            Map(x => x.Quantity);
        }
    }

    public interface IOfferedRepository : IRepository<Offered>
    {
        Offered CreateOrUpdate(CropUnit cu, Price p, double amount, Availability avail, int id);
    }

    public class OfferedRepository : Repository<Offered>, IOfferedRepository
    {
        public Offered CreateOrUpdate(CropUnit cu, Price p, double amount, Availability avail, int id = -1)
        {
            if (id == -1)
            {
                Offered offer = new Offered();
                offer.Quantity = amount;
                offer.CropPrice = p;
                offer.Availability = avail;

                this.EnsurePersistent(offer);
                return offer;
            }
            else
            {
                Offered offer = this.GetById(id);

                // Update amount if that's all that was changed
                /// TODO: Remember to check ordered quantities after changing total available quantity
                /// and notify everyone via e-mail somehow if the quantities are now less than what's
                /// been ordered.
                if (offer.Quantity != amount)
                {
                    offer.Quantity = amount;
                }

                // Update price if crop/unit weren't changed.
                if (offer.CropPrice.Id != p.Id && offer.CropPrice.CropUnit.Id == p.CropUnit.Id)
                {
                    offer.CropPrice = p;
                }

                // Make a completely new offer if either pack size or product were changed.
                else if (offer.CropPrice.Id != p.Id)
                {
                    this.Remove(offer);
                    return this.CreateOrUpdate(cu, p, amount, avail);
                }

                this.EnsurePersistent(offer);
                return offer;
            }
        }
    }
}