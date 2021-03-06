﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Net.Core.EntityModels.Queues;

namespace Net.Core.Repositories.Configuration
{
    internal class EmailQueueConfiguration : IEntityTypeConfiguration<EmailQueue>
    {
        public void Configure(EntityTypeBuilder<EmailQueue> builder)
        {
            builder.ToTable("EmailQueues").HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasColumnName("Id")
                .HasColumnType("bigint")
                .ValueGeneratedOnAdd()
                .IsRequired();

            builder.Property(x => x.FromEmailId)
                .HasColumnName("FromEmailId")
                .HasColumnType("nvarchar(500)")
                .IsRequired();

            builder.Property(x => x.ToEmailId)
                .HasColumnName("ToEmailId")
                .HasColumnType("nvarchar(500)")
                .IsRequired();

            builder.Property(x => x.EmailSubject)
                .HasColumnName("EmailSubject")
                .HasColumnType("nvarchar(500)")
                .IsRequired();

            builder.Property(x => x.MessageBody)
                .HasColumnName("MessageBody")
                .HasColumnType("nvarchar(MAX)")
                .IsRequired();

            builder.Property(x => x.AttachedFiles)
                .HasColumnName("AttachedFiles")
                .HasColumnType("nvarchar(MAX)");

            builder.Property(x => x.IsSucceedEmailSent)
                .HasColumnName("IsSucceedEmailSent")
                .HasColumnType("bit")
                .IsRequired();

            builder.Property(x => x.ErrorMessage)
                .HasColumnName("ErrorMessage")
                .HasColumnType("nvarchar(MAX)");
        }
    }
}
