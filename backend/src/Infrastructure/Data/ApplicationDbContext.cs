using System.Reflection;
using Sige.Application.Common.Interfaces;
using Sige.Domain.Entities;
using Sige.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Sige.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Emergencia> Emergencias => Set<Emergencia>();

    public DbSet<UnidadRespuesta> UnidadesRespuesta => Set<UnidadRespuesta>();

    public DbSet<Asignacion> Asignaciones => Set<Asignacion>();

    public DbSet<EventoAuditoria> EventosAuditoria => Set<EventoAuditoria>();

    public DbSet<TipoEmergencia> TiposEmergencia => Set<TipoEmergencia>();

    public DbSet<Institucion> Instituciones => Set<Institucion>();

    public DbSet<Personal> Personal => Set<Personal>();

    public DbSet<Recurso> Recursos => Set<Recurso>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
