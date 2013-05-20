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

        // Returns a comma-separated list of buyer names
        public virtual String GetBuyerNames()
        {
            int numBuyers = this.Buyers.Count;
            if (numBuyers == 1)
            {
                return this.Buyers.First().Buyer.Name;
            }
            else if (numBuyers > 0)
            {
                List<String> buyers = new List<String>();

                foreach (BuyerAvailability buyer in this.Buyers)
                {
                     buyers.Add(buyer.Buyer.Name);
                }

                return String.Join(", ", buyers);
            }

            return "";
        }

        // Checks whether or not buyer with id Id was assigned this availability.
        public virtual BuyerAvailability HasBuyer(int Id)
        {
            foreach (BuyerAvailability buyer in this.Buyers)
            {
                if (buyer.Buyer.Id == Id)
                {
                    return buyer;
                }
            }

            return null;
        }

        public virtual void UpdateBuyers(int[] buyers, Dictionary<int, int> dBuyer,
            String[] start_d, String[] start_t, String[] end_d, String[] end_t)
        {
            buyers = buyers ?? new int[0];

            IBuyerAvailabilityRepository buyerAvailRepo = new BuyerAvailabilityRepository();
            IRepository<Buyer> buyerRepo = new Repository<Buyer>();

            for (var i = 0; i < buyers.Length; i++)
            {    
                int buyerId = buyers[i];
                int pos = dBuyer[buyerId];

                buyerAvailRepo.CreateOrUpdate(buyerId, this, buyerRepo.GetById(buyerId),
                    start_d[pos], start_t[pos], end_d[pos], end_t[pos]);
            }
        }

        public virtual void CreateOrUpdateOffer(CropUnit cu, Price p, double amount, int id = -1)
        {
            IOfferedRepository offeredRepo = new OfferedRepository();

            offeredRepo.CreateOrUpdate(cu, p, amount, this, id);
        }
    }

    public class AvailabilityMap : ClassMap<Availability>
    {
        public AvailabilityMap()
        {
            Id(x => x.Id).GeneratedBy.Identity();
            Map(x => x.DateStart);
            Map(x => x.DateEnd);
            Map(x => x.Comments).Nullable();
            HasMany(x => x.Buyers)
                .Cascade.AllDeleteOrphan()
                .Inverse();
                // .Fetch.Join()
                // .KeyColumn("AvailabilityId");
            HasMany(x => x.Offered)
                .Cascade.AllDeleteOrphan()
                .Inverse();
                // .Fetch.Join();
                // .KeyColumn("AvailabilityId");
        }
    }
}