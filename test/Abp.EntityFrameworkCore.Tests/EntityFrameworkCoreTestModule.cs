﻿using System;
using Abp.Domain.Repositories;
using Abp.EntityFrameworkCore.Tests.Domain;
using Abp.EntityFrameworkCore.Tests.Ef;
using Abp.Modules;
using Abp.TestBase;
using Castle.MicroKernel.Registration;
using Castle.Windsor.MsDependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Abp.Configuration.Startup;
using Abp.Reflection.Extensions;

namespace Abp.EntityFrameworkCore.Tests
{
    [DependsOn(typeof(AbpEntityFrameworkCoreModule), typeof(AbpTestBaseModule))]
    public class EntityFrameworkCoreTestModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.UnitOfWork.IsTransactional = false; //EF Core InMemory DB does not support transactions

            var services = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase();

            var serviceProvider = WindsorRegistrationHelper.CreateServiceProvider(
                IocManager.IocContainer,
                services
            );

            //BloggingDbContext
            var blogDbOptionsBuilder = new DbContextOptionsBuilder<BloggingDbContext>();
            blogDbOptionsBuilder.UseInMemoryDatabase()
                .UseInternalServiceProvider(serviceProvider);

            IocManager.IocContainer.Register(
                Component
                    .For<DbContextOptions<BloggingDbContext>>()
                    .Instance(blogDbOptionsBuilder.Options)
                    .LifestyleSingleton()
            );

            //SupportDbContext
            var supportDbOptionsBuilder = new DbContextOptionsBuilder<SupportDbContext>();
            supportDbOptionsBuilder.UseInMemoryDatabase()
                .UseInternalServiceProvider(serviceProvider);

            IocManager.IocContainer.Register(
                Component
                    .For<DbContextOptions<SupportDbContext>>()
                    .Instance(supportDbOptionsBuilder.Options)
                    .LifestyleSingleton()
            );

            //Custom repository
            Configuration.ReplaceService<IRepository<Post, Guid>>(() =>
            {
                IocManager.IocContainer.Register(
                    Component.For<IRepository<Post, Guid>, IPostRepository, PostRepository>()
                        .ImplementedBy<PostRepository>()
                        .LifestyleTransient()
                );
            });
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(EntityFrameworkCoreTestModule).GetAssembly());
        }
    }
}