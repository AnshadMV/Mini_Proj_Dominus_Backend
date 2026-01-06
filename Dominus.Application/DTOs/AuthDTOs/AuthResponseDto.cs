using System.Text.Json.Serialization;

namespace Dominus.Application.DTOs.AuthDTOs
{
    public class AuthResponseDto
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = null!;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? AccessToken { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? RefreshToken { get; set; }
        public AuthResponseDto(
            int statusCode,
            string message,
            string? accessToken = null,
            string? refreshToken = null)
        {
            StatusCode = statusCode;
            Message = message;
            AccessToken = accessToken;
            RefreshToken = refreshToken;
        }

        //public AuthResponseDto(int statusCode, string message)
        //{
        //    StatusCode = statusCode;
        //    Message = message;
        //}

        //public AuthResponseDto(int statusCode, string message, string accessToken)
        //{
        //    StatusCode = statusCode;
        //    Message = message;
        //    AccessToken = accessToken;
        //}

        //public AuthResponseDto(int statusCode, string message, string accessToken, string refreshToken)
        //{
        //    StatusCode = statusCode;
        //    Message = message;
        //    AccessToken = accessToken;
        //    RefreshToken = refreshToken;
        //}

    }
}






