using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using BlogWebMVCIdentityAuth.Data;

namespace BlogWebMVCIdentityAuth.Models
{
    [Table("Topic")]
    public class Topic {

        public int Id { get; set; }

        public String Name { get; set; }

        public String ImageURL { get; set; }

        [InverseProperty("Topic")]
        public ICollection<Blog> Blogs { get; set; }
        

        public static void SeedData(ApplicationDbContext context) {
            if(!context.Topics.Any()) {
                
                context.AddRange(

                    new Topic() {Name="News", ImageURL="/images/topic-news.jpg"},
                    new Topic() {Name="Foods & Drinks", ImageURL="/images/topic-food-and-drink.jpg"},
                    new Topic() {Name="Pets & Animals", ImageURL="/images/topic-pet-and-animal.jpg"},
                    new Topic() {Name="Fashion & Design", ImageURL="/images/topic-fashion-and-design.jpg"},
                    new Topic() {Name="Health & Wellness", ImageURL="/images/topic-health-and-wellness.jpg"},
                    new Topic() {Name="People", ImageURL="/images/topic-people.jpg"},
                    new Topic() {Name="Natural", ImageURL="/images/topic-nature.jpg"},
                    new Topic() {Name="Cultural", ImageURL="/images/topic-cultural.jpg"},
                    new Topic() {Name="Travel & Explore", ImageURL="/images/topic-travel-and-explore.jpg"},
                    new Topic() {Name="Other", ImageURL="/images/topic-other.jpg"}
                );

                context.SaveChanges();
                
            }
        }

    }
    
}