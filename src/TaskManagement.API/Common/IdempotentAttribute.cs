using Microsoft.AspNetCore.Mvc;
namespace TaskManagement.API.Common;

public class IdempotentAttribute() : TypeFilterAttribute(typeof(IdempotencyFilter));