using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UCDArch.Core.DomainModel;

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
}