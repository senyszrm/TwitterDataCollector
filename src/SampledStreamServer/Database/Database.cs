using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SampledStreamServer.Database
{
    public class SampledStreamDbContext : DbContext
    {
        [NotMapped]
        private string databasePath;

        // SummaryData is expected to only have 1 row, this is private so that the one row can be managed below
        private DbSet<SummaryData> SummaryDatas { get; set; } = null!;

        // Gets the first row from the SummaryDatas table as we only want to work with that one row
        public SummaryData SummaryData {
            get
            {
                SummaryData summaryData = SummaryDatas.FirstOrDefault() ?? new SummaryData();
                if (SummaryDatas.FirstOrDefault() == null)
                {
                    this.SummaryDatas.Add(summaryData);
                    this.SaveChanges();
                }
                return summaryData;
            }
        }

        public DbSet<Hashtag> Hashtags { get; set; } = null!;

        public SampledStreamDbContext(string databasePath = "sampled_stream.db")
        {
            this.databasePath = databasePath;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(String.Format("Data Source={0}", this.databasePath));
        }

    }

    public class SummaryData
    {
        public int SummaryDataId { get; set; }
        public uint TotalTweets { get; set; } = 0;

        public List<KeyValuePair<string, uint>> GetTopTenHashtags(SampledStreamDbContext db)
        {
            return db.Hashtags.Include(h => h.ObservedTimes).ToList().OrderByDescending(h => h.ObservedTimes.Count).Take(10).ToList().Select(h => new KeyValuePair<string, uint>(h.Name, (uint)(h.ObservedTimes.Count))).ToList();
        }

    }

    public class Hashtag
    {
        public int HashtagId { get; set; }
        public string Name { get; set; } = null!;

        public List<HashtagOccurrence> ObservedTimes { get; set; } = null!;
    }

    public class HashtagOccurrence
    {
        public int HashtagOccurrenceId { get; set; }
        public DateTime TimeOfOccurrence { get; set; }
        public virtual Hashtag Hashtag { get; set; } = null!;
    }
}
