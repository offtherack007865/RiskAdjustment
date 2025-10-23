using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiskAdjustment.Data;
using RiskAdjustment.Data.Models.RefTables;

namespace RiskAdjustment.Logic.Global
{
    public interface ISiteLogic
    {
        Site GetSite(string siteNumber);
        Site GetSite(int Id);
        List<Site> GetSites(List<int> siteIds);
        List<Site> GetSites(List<string> siteNumbers);
        List<Site> GetAllSites();
        int UpdateSite(NameValueCollection values);
        int AddSite(NameValueCollection values);

    }
    public class SiteLogic : ISiteLogic
    {
        public Site GetSite(string siteNumber)
        {
            string sql = $"SELECT * FROM Sites WHERE SiteNumber = '{siteNumber}'";
            DbQuery query = new DbQuery();
            return query.ExecuteSingle<Site>(sql);
        }

        public Site GetSite(int id)
        {
            string sql = $"SELECT * FROM Sites WHERE Id = {id}";
            DbQuery query = new DbQuery();
            return query.ExecuteSingle<Site>(sql); ;
        }

        public List<Site> GetSites(List<int> siteIds)
        {
            throw new NotImplementedException();
        }

        public List<Site> GetSites(List<string> siteNumbers)
        {
            throw new NotImplementedException();
        }

        public List<Site> GetAllSites()
        {
            string sql = "SELECT * FROM Sites WHERE Active = 1";
            DbQuery query = new DbQuery();
            return query.Execute<Site>(sql).ToList();
        }

        public int UpdateSite(NameValueCollection form)
        {
            Site site = ParseSiteFormParams(form);
            string sql =
                $"UPDATE Sites SET SiteNumber = '{site.SiteNumber}', Name = '{site.Name}', County = '{site.County}' WHERE Id = {site.Id}";
            DbQuery query = new DbQuery();
            return query.ExecuteCommand(sql);
            
        }

        public int AddSite(NameValueCollection values)
        {
            values = SanatizeNewUserFormParams(values);
            Site site = ParseSiteFormParams(values);
            string sql =
                $"INSERT INTO Sites (SiteNumber, Name, County, Active) VALUES ('{site.SiteNumber}', '{site.Name}', '{site.County}', 1)";
            DbQuery query = new DbQuery();
            return query.ExecuteCommand(sql);
        }

        private Site ParseSiteFormParams(NameValueCollection values)
        {
            Site site = new Site()
            {
                SiteNumber = values["SiteNumber"].Trim('"'),
                Name = values["Name"].Trim('"').Replace("'", ""),
                County = values["County"].Trim('"')
            };
            if (values["Id"] != null)
            {
                site.Id = Convert.ToInt32(values["Id"].Trim('"'));
            }
            return site;


        }

        private NameValueCollection SanatizeNewUserFormParams(NameValueCollection values)
        {
            NameValueCollection returnValue = new NameValueCollection();
            for (int i = 0; i < values.Count; i++)
            {
                string newKey = values.GetKey(i).Remove(0, 4);
                returnValue.Add(newKey, values[i]);
            }

            return returnValue;
        }
    }
}
