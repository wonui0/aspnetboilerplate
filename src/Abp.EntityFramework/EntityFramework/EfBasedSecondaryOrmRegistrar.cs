﻿using System;
using Abp.Dependency;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Orm;
using Abp.Reflection.Extensions;

namespace Abp.EntityFramework
{
    public class EfBasedSecondaryOrmRegistrar : ISecondaryOrmRegistrar
    {
        private readonly Type _dbContextType;
        private readonly IDbContextEntityFinder _dbContextEntityFinder;

        public EfBasedSecondaryOrmRegistrar(Type dbContextType, IDbContextEntityFinder dbContextEntityFinder)
        {
            _dbContextType = dbContextType;
            _dbContextEntityFinder = dbContextEntityFinder;
        }

        public string OrmContextKey => AbpConsts.Orms.EntityFramework;

        public void RegisterRepositories(IIocManager iocManager, AutoRepositoryTypesAttribute defaultRepositoryTypes)
        {
            AutoRepositoryTypesAttribute autoRepositoryAttr = _dbContextType.GetSingleAttributeOrNull<AutoRepositoryTypesAttribute>()
                                                              ?? defaultRepositoryTypes;

            foreach (EntityTypeInfo entityTypeInfo in _dbContextEntityFinder.GetEntityTypeInfos(_dbContextType))
            {
                Type primaryKeyType = EntityHelper.GetPrimaryKeyType(entityTypeInfo.EntityType);
                if (primaryKeyType == typeof(int))
                {
                    Type genericRepositoryType = autoRepositoryAttr.RepositoryInterface.MakeGenericType(entityTypeInfo.EntityType);
                    if (!iocManager.IsRegistered(genericRepositoryType))
                    {
                        Type implType = autoRepositoryAttr.RepositoryImplementation.GetGenericArguments().Length == 1
                            ? autoRepositoryAttr.RepositoryImplementation.MakeGenericType(entityTypeInfo.EntityType)
                            : autoRepositoryAttr.RepositoryImplementation.MakeGenericType(entityTypeInfo.DeclaringType, entityTypeInfo.EntityType);

                        iocManager.Register(
                            genericRepositoryType,
                            implType,
                            DependencyLifeStyle.Transient
                        );
                    }
                }

                Type genericRepositoryTypeWithPrimaryKey = autoRepositoryAttr.RepositoryInterfaceWithPrimaryKey.MakeGenericType(entityTypeInfo.EntityType, primaryKeyType);
                if (!iocManager.IsRegistered(genericRepositoryTypeWithPrimaryKey))
                {
                    Type implType = autoRepositoryAttr.RepositoryImplementationWithPrimaryKey.GetGenericArguments().Length == 2
                        ? autoRepositoryAttr.RepositoryImplementationWithPrimaryKey.MakeGenericType(entityTypeInfo.EntityType, primaryKeyType)
                        : autoRepositoryAttr.RepositoryImplementationWithPrimaryKey.MakeGenericType(entityTypeInfo.DeclaringType, entityTypeInfo.EntityType, primaryKeyType);

                    iocManager.Register(
                        genericRepositoryTypeWithPrimaryKey,
                        implType,
                        DependencyLifeStyle.Transient
                    );
                }
            }
        }
    }
}
