﻿using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace DNWS
{
    class TwitterApiPlugin : TwitterPlugin
    {
        public string[] Test()
        {
            return new string[]
            {
                "Hello,",
                "World!"
            };
        }

        public List<User> GetAllUsers()
        {
            using (var context = new TweetContext())
            {
                try
                {
                    List<User> users = context.Users.Where(b => true).Include(b => b.Following).ToList();
                    return users;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public List<Following> GetFollowing(string name)
        {
            using (var context = new TweetContext())
            {
                try
                {
                    List<User> followings = context.Users.Where(b => b.Name.Equals(name)).Include(b => b.Following).ToList();
                    return followings[0].Following;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
        
        public override HTTPResponse GetResponse(HTTPRequest request)
        {           
            HTTPResponse response = new HTTPResponse(200);
            string user = request.getRequestByKey("user");
            string password = request.getRequestByKey("password");
            string following = request.getRequestByKey("follow");
            string message = request.getRequestByKey("message");
            string[] path = request.Filename.Split("?");
            if (path[0] == "users")
            {     
                if (request.Method == "GET")
                {
                    string json = JsonConvert.SerializeObject(GetAllUsers());
                    response.body = Encoding.UTF8.GetBytes(json);
                }else if(request.Method == "POST")
                {
                    try
                    {
                        Twitter.AddUser(user, password);
                        response.body = Encoding.UTF8.GetBytes("200 OK");
                    }
                    catch (Exception)
                    {
                        response.status = 403;
                        response.body = Encoding.UTF8.GetBytes("403 User already exists");
                    }           
                }else if(request.Method == "DELETE")
                {
                    Twitter twitter = new Twitter(user);
                    try
                    {
                        twitter.DeleteUser(user);
                        response.body = Encoding.UTF8.GetBytes("200 OK");
                    }
                    catch (Exception)
                    {
                        response.status = 404;
                        response.body = Encoding.UTF8.GetBytes("404 User not exists");
                    }
                }
            }else if (path[0] == "following")
            {
                Twitter twitter = new Twitter(user);
                if (request.Method == "GET")
                {
                    string json = JsonConvert.SerializeObject(GetFollowing(user));
                    response.body = Encoding.UTF8.GetBytes(json);
                }
                else if (request.Method == "POST")
                {
                    if (Twitter.CheckUser(following))
                    {
                        twitter.AddFollowing(following);
                        response.body = Encoding.UTF8.GetBytes("200 OK");
                    }
                    else
                    {
                        response.status = 404;
                        response.body = Encoding.UTF8.GetBytes("404 User not exists");
                    }
                }
                else if (request.Method == "DELETE")
                {
                    try
                    {
                        twitter.RemoveFollowing(following);
                        response.body = Encoding.UTF8.GetBytes("200 OK");
                    }
                    catch (Exception)
                    {
                        response.status = 404;
                        response.body = Encoding.UTF8.GetBytes("404 User not exists");
                    }
                }
            }else if(path[0] == "tweets")
            {
                Twitter twitter = new Twitter(user);
                if (request.Method == "GET")
                {
                    try { 
                        string timeline = request.getRequestByKey("timeline");
                        if (timeline == "following") {
                            string json = JsonConvert.SerializeObject(twitter.GetFollowingTimeline());
                            response.body = Encoding.UTF8.GetBytes(json);
                        }
                        else {   
                            string json = JsonConvert.SerializeObject(twitter.GetUserTimeline());
                            response.body = Encoding.UTF8.GetBytes(json);
                        }
                    }
                    catch (Exception)
                    {
                        response.status = 404;
                        response.body = Encoding.UTF8.GetBytes("404 User not found");
                    }
                }
                else if (request.Method == "POST")
                {
                    try
                    {
                        twitter.PostTweet(message);
                        response.body = Encoding.UTF8.GetBytes("200 OK");
                    }
                    catch (Exception) {
                        response.status = 404;
                        response.body = Encoding.UTF8.GetBytes("404 User not found");
                    }
                }
            }
            response.type = "application/json";         
            return response;
        }       
    }
}
