using System.Data;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.Dtos;
using DotnetAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DotnetAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class PostController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        public PostController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
        }

        [HttpGet("Posts/{postId}/{userId}/{searchParam}")]
        public IEnumerable<Post> GetPosts(int postId = 0, int userId = 0, string searchParam = "None")
        {
            string sql = @"EXEC TutorialAppSchema.spPosts_Get";
            string parameters = "";
            DynamicParameters sqlParameters = new DynamicParameters();

            if (postId != 0)
            {
                parameters += ", @PostId= @PostIdParameters";
                sqlParameters.Add("@PostIdParameters", postId, DbType.Int32);
                
            }
            if (userId != 0)
            {
                parameters += ", @UserId=@UserIdParameters";
                sqlParameters.Add("@UserIdParameters", userId, DbType.Int32);
            }
            if (searchParam.ToLower() != "none")
            {
                parameters += ", @SearchValue=@SearchValueParameters";
                sqlParameters.Add("@SearchValueParameters", searchParam, DbType.String);
            }

            if (parameters.Length > 0)
            { 
                sql += parameters.Substring(1);
            }
                
            return _dapper.LoadDataWithParameters<Post>(sql, sqlParameters);
        }

        [HttpGet("MyPosts")]
        public IEnumerable<Post> GetMyPosts()
        {
            string sql = @"EXEC TutorialAppSchema.spPosts_Get @UserId =@userIdIdParameters" ;

            DynamicParameters sqlParameters = new DynamicParameters();
            sqlParameters.Add("@userIdIdParameters", this.User.FindFirst("userId")?.Value, DbType.Int32);

                
             return _dapper.LoadDataWithParameters<Post>(sql, sqlParameters);
        }

        [HttpPut("UpsertPost")]
        public IActionResult UpsertPost(Post postToUpsert)
        {
            string sql = @"EXEC TutorialAppSchema.spPosts_Upsert
                @UserId =@UserIdParameters,
                @PostTitle = @PostTitleParameters,
                @PostContent =@PostContentParameters";

            DynamicParameters sqlParameters = new DynamicParameters();
            sqlParameters.Add("@UserIdParameters", this.User.FindFirst("userId")?.Value, DbType.Int32);
            sqlParameters.Add("@PostTitleParameters",postToUpsert.PostTitle, DbType.String);
            sqlParameters.Add("@PostContentParameters",postToUpsert.PostContent, DbType.String);
           
           
           
            if (postToUpsert.PostId > 0) {
                sql +=  ", @PostId = @PostIdParameters";
                sqlParameters.Add("@PostIdParameters",postToUpsert.PostId, DbType.Int32);
            }

            if (_dapper.ExecuteSqlWithParameters(sql,sqlParameters))
            {
                return Ok();
            }

            throw new Exception("Failed to upsert post!");
        }


        [HttpDelete("Post/{postId}")]
        public IActionResult DeletePost(int postId)
        {
            string sql = @"EXEC TutorialAppSchema.spPost_Delete 
            @PostId = @PostIdParameters";

            DynamicParameters sqlParameters = new DynamicParameters();
            sqlParameters.Add("@PostIdParameters", postId, DbType.Int32);

            
            if (_dapper.ExecuteSqlWithParameters(sql, sqlParameters))
            {
                return Ok();
            }

            throw new Exception("Failed to delete post!");
        }
    }
}