﻿// <auto-generated />
using System;
using API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace API.Data.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20240531174657_Initial")]
    partial class Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.20");

            modelBuilder.Entity("API.Models.User", b =>
                {
                    b.Property<Guid>("Guid")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<bool>("Admin")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("Birthday")
                        .HasColumnType("TEXT");

                    b.Property<string>("CreatedBy")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("TEXT");

                    b.Property<int>("Gender")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Login")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ModifiedBy")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("ModifiedOn")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("RevokedBy")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("RevokedOn")
                        .HasColumnType("TEXT");

                    b.HasKey("Guid");

                    b.ToTable("Users");
                });
#pragma warning restore 612, 618
        }
    }
}
