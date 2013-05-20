using Castle.Windsor;
using UCDArch.Core.CommonValidator;
using UCDArch.Core.DataAnnotationsValidator.CommonValidatorAdapter;
using UCDArch.Core.PersistanceSupport;
using UCDArch.Data.NHibernate;
using Castle.MicroKernel.Registration;
using StudentFarm.Models;

namespace StudentFarm
{
    internal static class ComponentRegistrar
    {
        public static void AddComponentsTo(IWindsorContainer container)
        {
            AddGenericRepositoriesTo(container);
            AddRepositoriesTo(container);

            container.Register(Component.For<IValidator>().ImplementedBy<Validator>().Named("validator"));
            container.Register(Component.For<IDbContext>().ImplementedBy<DbContext>().Named("dbContext"));
        }

        private static void AddGenericRepositoriesTo(IWindsorContainer container)
        {
            container.Register(Component.For(typeof(IRepositoryWithTypedId<,>)).ImplementedBy(typeof(RepositoryWithTypedId<,>)).Named("repositoryWithTypedId"));
            container.Register(Component.For(typeof(IRepository<>)).ImplementedBy(typeof(Repository<>)).Named("repositoryType"));
            container.Register(Component.For<IRepository>().ImplementedBy<Repository>().Named("repository"));
        }

        private static void AddRepositoriesTo(IWindsorContainer container)
        {
            container.Register(Component.For<ICropRepository>().ImplementedBy<CropRepository>());
            container.Register(Component.For<IUnitRepository>().ImplementedBy<UnitRepository>());
            container.Register(Component.For<ICropUnitRepository>().ImplementedBy<CropUnitRepository>());
            container.Register(Component.For<IPriceRepository>().ImplementedBy<PriceRepository>());
            container.Register(Component.For<IBuyerAvailabilityRepository>().ImplementedBy<BuyerAvailabilityRepository>());
        }
    }
}