using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using LavaderoMotos.Data;
using Microsoft.EntityFrameworkCore;

namespace LavaderoMotos.Models
{
    public class UtcDateInterceptor : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            ConvertDateTimesToUtc(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            ConvertDateTimesToUtc(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void ConvertDateTimesToUtc(DbContext? context)
        {
            if (context == null) return;

            var entries = context.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                foreach (var property in entry.Properties)
                {
                    // Para DateTime
                    if (property.Metadata.ClrType == typeof(DateTime) && property.CurrentValue is DateTime dt)
                    {
                        if (dt.Kind != DateTimeKind.Utc)
                        {
                            property.CurrentValue = dt.ToUniversalTime();
                        }
                    }
                    
                    // Para DateTime? - FORMA CORREGIDA
                    if (property.Metadata.ClrType == typeof(DateTime?) && property.CurrentValue is DateTime nullableDt)
                    {
                        if (nullableDt.Kind != DateTimeKind.Utc)
                        {
                            property.CurrentValue = nullableDt.ToUniversalTime();
                        }
                    }
                }
            }
        }
    }
}