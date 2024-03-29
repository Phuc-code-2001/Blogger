﻿using System;
using System.Collections.Generic;
using System.Text;
using BlogWebMVCIdentityAuth.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlogWebMVCIdentityAuth.Data
{
    public class ApplicationDbContext : IdentityDbContext<Author, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            
        }
        protected override void OnModelCreating (ModelBuilder builder) {

            base.OnModelCreating (builder); 
            // Bỏ tiền tố AspNet của các bảng: mặc định các bảng trong IdentityDbContext có
            // tên với tiền tố AspNet như: AspNetUserRoles, AspNetUser ...
            // Đoạn mã sau chạy khi khởi tạo DbContext, tạo database sẽ loại bỏ tiền tố đó
            foreach (var entityType in builder.Model.GetEntityTypes ()) {
                var tableName = entityType.GetTableName ();
                if (tableName.StartsWith ("AspNet")) {
                    entityType.SetTableName (tableName.Substring(6));
                }
            }
        }

        public DbSet<Topic> Topics { get; set; }

        public DbSet<Blog> Blogs { get; set; }

        public DbSet<Like> LikeObjects { get; set; }

        public DbSet<Following> FollowObjects { get; set; }
    }
}
