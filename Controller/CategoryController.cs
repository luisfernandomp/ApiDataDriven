using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;

namespace Shop.Controllers{

    [Route("v1/categories")]
    public class CategoryController : ControllerBase
    {
        [HttpGet]
        [Route("")]
        // ResponseCacheLocation pode ser Cliente, Any ou None
        // [ResponseCache(VaryByHeader = "User-Agent", Location = ResponseCacheLocation.Any, Duration = 30)]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)] // desabilita o cache para esse método
        public async Task<ActionResult<List<Category>>> Get([FromServices]DataContext context){
            //  AsNoTracking -> faz uma leitura da forma mais rápida possível sem trazer a proxy que é de uso exclusivo do EF Core
            // ToList sempre no final, pois nessa hora o EF Core já foi no banco buscar as informações

            var categories = await context.Categories.AsNoTracking().ToListAsync();

            return Ok(categories);
        }

        [HttpGet]
        [Route("{id:int}")]
        public async Task<ActionResult<Category>> GetById(
                int id, 
                [FromServices]DataContext context
            ){
            var category = await context.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);

            return Ok(category);
        }


        [HttpPost]
        [Route("")]
        public async Task<ActionResult<List<Category>>> Post( [FromBody]Category category,
                [FromServices] DataContext context){
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                context.Categories.Add(category); // gera um id automático
                await context.SaveChangesAsync();
                return Ok(category);
            }
            catch 
            {
                return BadRequest( new { message = "Não foi possível adicionar essa categoria"});
            }

            
        }

        [HttpPut]
        [Route("{id:int}")]
        public async Task<ActionResult<List<Category>>> Put(
            int id, 
            [FromBody]Category category,
            [FromServices]DataContext context
        )
            {
            if(id != category.Id)
                return NotFound( new { message = "Categoria não encontrada"});
            
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                context.Entry<Category>(category).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                await context.SaveChangesAsync();
                return Ok(category);
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest(new { message = "Este registro já foi atualizado"});
            }
            catch(Exception){
                return BadRequest(new { message = "Não foi possível atualizar a categoria"});
            }
        }

        [HttpDelete]
        [Route("{id:int}")]
        async Task<ActionResult<List<Category>>> Delete(int id, [FromServices]DataContext context){
            var category = await context.Categories.FirstOrDefaultAsync(x => x.Id == id);
            // FirstOrDefaultAsync -> retorna o primeiro registro encontrado se forem encontrados mais de um e um null se nada for encontrado

            if(category == null)
                return NotFound(new { message = "Categoria não encontrada"});

            try
            {
                context.Categories.Remove(category);
                await context.SaveChangesAsync();
                return Ok(new {message = "Categoria removida com sucesso!"});
            }
            catch (System.Exception)
            {
                return BadRequest(new {message = "Não foi possível excluir a categoria informada"});
            }
        }


    }
}