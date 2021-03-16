using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DuplicateLogin.Util
{
    public class LoginHelper
    {
        private IMemoryCache _siteCache = null;
        private readonly IConfiguration _configuration;


        public LoginHelper(IMemoryCache Cache, IConfiguration configuration)
        {
            _siteCache = Cache;
            _configuration = configuration;
        }

        /// <summary>
        /// this method is called from a middleware through which every request passes
        /// </summary>
        /// <param name="Context"></param>
        public void CheckSingleLogin(HttpContext Context)
        {
            var claim = Context.User;
            var username = claim?.FindFirst(ClaimTypes.Name)?.Value;

            //this is meant to refresh the session time in the cache memory
            IsDuplicateLogin(username);
        }

        /// <summary>
        /// Checks if the user is logged in already, resulting in duplicate login
        /// </summary>
        /// <param name="UserName"></param>
        /// <param name="IsFromLogin"></param>
        /// <returns></returns>
        public bool IsDuplicateLogin(string UserName, bool IsFromLogin = false)
        {
            if (string.IsNullOrEmpty(UserName))
                return false;

            String sKey = UserName.ToLower();
            String sUser = Convert.ToString(_siteCache.Get<string>(sKey));

            //returns true if this is from a login page AND the user already has session in memory
            if (!string.IsNullOrEmpty(sUser) && IsFromLogin)
                return true;

            if (!string.IsNullOrEmpty(sUser) || IsFromLogin)
            {

                var SessionTimeoutMinutes = _configuration["SessionTimeoutMinutes"] ?? "5";
                int timeout = int.Parse(SessionTimeoutMinutes);

                sUser = string.Format("{0} - {1} min from {2}", sKey, timeout, DateTime.Now.AddMinutes(timeout).ToString("yyyy-MM-dd HH:mm:ss"));

                TimeSpan slidingTimeout = new TimeSpan(0, 0, timeout, 0, 0);

                MemoryCacheEntryOptions cacheOptions = new MemoryCacheEntryOptions
                {
                    // Keep in cache for this time, reset time if accessed again.
                    //sliding expiration, rather than absolute expiration is used since session time is reset with each http request
                    SlidingExpiration = slidingTimeout

                };

                _siteCache.Set(sKey, sUser, cacheOptions);

                //returns false if this comes from a login page 
                //at this point the user trying to log in does not have a session in memory
                //if not from a login page, then return true since the user has a session in memory
                return IsFromLogin ? false : true;
            }
            else
            {
                // return false since the user does not have a session in memory and he's not trying to login
                return false;
            }
        }

        /// <summary>
        /// Removes a user login from the memory when the user logs out
        /// </summary>
        /// <param name="UserName"></param>
        public void RemoveLogin(string UserName)
        {
            UserName = UserName?.ToLower();

            //Clear the cache
            if ((!string.IsNullOrEmpty(UserName)) && _siteCache != null)
            {
                var sUser = _siteCache.Get<string>(UserName);

                if (!string.IsNullOrEmpty(sUser))
                {
                    _siteCache.Remove(UserName);
                }
            }
        }
    }
}
