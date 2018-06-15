using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace SpiderAirlineCompany
{
    public class AppDbContext:DbContext
    {
        public AppDbContext():base()
        {

        }
        public DbSet<AirlineCompany> AirlineCompany { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //配置mariadb连接字符串
            optionsBuilder.UseSqlServer(@"Data Source=A-PC;Initial Catalog=testdb;Integrated Security=False;User ID=sa;Password=1234;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
        }
    }
}
