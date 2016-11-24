﻿// ==========================================================================
//  SchemasController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Modules.Api.Schemas.Models;
using Squidex.Pipeline;
using Squidex.Read.Schemas.Repositories;
using Squidex.Write.Schemas.Commands;

namespace Squidex.Modules.Api.Schemas
{
    /// <summary>
    /// Manages and retrieves information about schemas.
    /// </summary>
    [Authorize("app-owner,app-developer")]
    [ApiExceptionFilter]
    [ServiceFilter(typeof(AppFilterAttribute))]
    [SwaggerTag("Schemas")]
    public class SchemasController : ControllerBase
    {
        private readonly ISchemaRepository schemaRepository;
        
        public SchemasController(ICommandBus commandBus, ISchemaRepository schemaRepository)
            : base(commandBus)
        {
            this.schemaRepository = schemaRepository;
        }

        /// <summary>
        /// Get app schemas.
        /// </summary>
        /// <returns>
        /// 200 => Schemas returned.
        /// </returns>
        [HttpGet]
        [Route("apps/{app}/schemas/")]
        [ProducesResponseType(typeof(SchemaDto[]), 200)]
        public async Task<IActionResult> GetSchemas()
        {
            var schemas = await schemaRepository.QueryAllAsync(AppId);

            var model = schemas.Select(s => SimpleMapper.Map(s, new SchemaDto())).ToList();

            return Ok(model);
        }

        /// <summary>
        /// Get a schema by name.
        /// </summary>
        /// <param name="name">The name of the schema to retrieve.</param>
        /// <returns>
        /// 200 => Schema found.
        /// 404 => Schema not found.
        /// </returns>
        [HttpGet]
        [Route("apps/{app}/schemas/{name}/")]
        [ProducesResponseType(typeof(SchemaDetailsDto[]), 200)]
        public async Task<IActionResult> GetSchema(string name)
        {
            var entity = await schemaRepository.FindSchemaAsync(AppId, name);

            if (entity == null)
            {
                return NotFound();
            }

            return Ok(null);
        }

        /// <summary>
        /// Create a new schema.
        /// </summary>
        /// <param name="model">The schema object that needs to be added to the app.</param>
        /// <param name="app">The name of the app to create the schema to.</param>
        /// <returns>
        /// 201 => Schema created.  
        /// 400 => Schema name is not valid.
        /// 409 => Schema name already in use.
        /// </returns>
        [HttpPost]
        [Route("apps/{app}/schemas/")]
        [ProducesResponseType(typeof(EntityCreatedDto), 201)]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ProducesResponseType(typeof(ErrorDto), 409)]
        public async Task<IActionResult> PostSchema(string app, [FromBody] CreateSchemaDto model)
        {
            var command = SimpleMapper.Map(model, new CreateSchema { AggregateId = Guid.NewGuid() });

            await CommandBus.PublishAsync(command);

            return CreatedAtAction(nameof(GetSchema), new { name = model.Name }, new EntityCreatedDto { Id = command.Name });
        }

        /// <summary>
        /// Update a schema.
        /// </summary>
        /// <param name="app">The app where the schema is a part of.</param>
        /// <param name="name">The name of the schema.</param>
        /// <param name="model">The schema object that needs to updated.</param>
        /// <returns>
        /// 204 => Schema updated.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/schemas/{name}/")]
        public async Task<IActionResult> PutSchema(string app, string name, [FromBody] UpdateSchemaDto model)
        {
            var command = SimpleMapper.Map(model, new UpdateSchema());

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        /// <summary>
        /// Delete a schema.
        /// </summary>
        /// <param name="app">The app where the schema is a part of.</param>
        /// <param name="name">The name of the schema to delete.</param>
        /// <returns>
        /// 204 => Schema has been deleted.
        /// </returns>
        [HttpDelete]
        [Route("apps/{app}/schemas/{name}/")]
        public async Task<IActionResult> DeleteSchema(string app, string name)
        {
            await CommandBus.PublishAsync(new DeleteSchema());

            return NoContent();
        }
    }
}