using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UCDArch.Core.DomainModel;
using UCDArch.Core.PersistanceSupport;
using UCDArch.Data.NHibernate;

namespace StudentFarm.Models
{
    public class Crop : DomainObjectWithTypedId<int>
    {
        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual bool Organic { get; set; }
        public virtual DateTime? AddedDate { get; set; }

        public Crop()
        {
        }
    }

    public class CropMap : ClassMap<Crop>
    {
        public CropMap()
        {
            Id(x => x.Id).GeneratedBy.Identity();
            Map(x => x.Name);
            Map(x => x.Description).Nullable();
            Map(x => x.Organic);
            Map(x => x.AddedDate).Nullable();
        }
    }

    public interface ICropRepository : IRepository<Crop>
    {
        Crop GetOrCreate(int id, String name, bool organic = true);
    }

    public class CropRepository : Repository<Crop>, ICropRepository
    {
        public Crop GetOrCreate(int id, String name, bool organic = true) {
            Crop crop = this.GetNullableById(id);

            if (crop == null || crop.Name.ToLower() != name.ToLower())
            {
                crop = new Crop();
                crop.Name = name;
                crop.Organic = organic; // Assume everything's organic unless specified otherwise 
                // (and one can specify otherwise via the crop controller)
                this.EnsurePersistent(crop);
            }

            return crop;
        }
    }
}