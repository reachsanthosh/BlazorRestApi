﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace BlazorApi.Data
{
    public static class SeedData
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userManager"></param>
        /// <param name="roleManager"></param>
        /// <returns></returns>
        public static async Task Seed(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            await SeedUsers(userManager);
            await SeedRoles(roleManager);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userManager"></param>
        /// <returns></returns>
        public static async Task SeedUsers(UserManager<IdentityUser> userManager)
        {
            if (await userManager.FindByEmailAsync("admin@bookstore.com") == null)
            {
                var user = new IdentityUser
                {
                    UserName = "admin",
                    Email = "admin@bookstore.com"
                };
                var result = await userManager.CreateAsync(user, "P@ssword1");
                if (result.Succeeded)
                {
                   await userManager.AddToRoleAsync(user, "Administrator");

                }
            }

            if (await userManager.FindByEmailAsync("customer1@gmail.com") == null)
            {
                var user = new IdentityUser
                {
                    UserName = "customer1",
                    Email = "customer1@gmail.com"
                };
                var result = await userManager.CreateAsync(user, "P@ssword1");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Customer1");

                }
            }

            if (await userManager.FindByEmailAsync("customer2@gmail.com") == null)
            {
                var user = new IdentityUser
                {
                    UserName = "customer2",
                    Email = "customer2@gmail.com"
                };
                var result = await userManager.CreateAsync(user, "P@ssword1");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Customer2");

                }
            }
        }
        public static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync("Administrator"))
            {
                var role = new IdentityRole
                {
                    Name = "Administrator"
                };

                await roleManager.CreateAsync(role);
            }

            if (!await roleManager.RoleExistsAsync("Customer"))
            {
                var role = new IdentityRole
                {
                    Name = "Customer"
                };

                await roleManager.CreateAsync(role);
            }

            
        }

    }
}
