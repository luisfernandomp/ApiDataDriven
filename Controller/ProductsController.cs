using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shop.Data;
using Shop.Models;

namespace Shop.Controllers{
    
    [Route("v1/products")]
    public class ProductsController : ControllerBase
    {
        [HttpGet]
        [Route("")]
        [AllowAnonymous] //Método anônimo, não exige autenticação
        public async Task<ActionResult<List<Product>>> Get([FromServices]DataContext context){
            //  AsNoTracking -> faz uma leitura da forma mais rápida possível sem trazer a proxy que é de uso exclusivo do EF Core
            // ToList sempre no final, pois nessa hora o EF Core já foi no banco buscar as informações

            var products = await context
                    .Products
                    .Include(x => x.Category)
                    .AsNoTracking()
                    .ToListAsync();

            return Ok(products);
        }

        [HttpGet]
        [Route("{id:int}")]
        [Authorize]
        public async Task<ActionResult<Product>> GetById(
                int id, 
                [FromServices]DataContext context
            ){
            var product = await context
                    .Products
                    .Include(y => y.Category) //Onde ocorre o JOIN na consulta
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id);

            return Ok(product);
        }

        [HttpGet] // products/categories/1
        [Route("categories/{id:int}")]
        [Authorize]
        public async Task<ActionResult<Product>> GetByCategory(
                int id, 
                [FromServices]DataContext context
            ){
            var products = await context
                    .Products
                    .Include(y => y.Category) //Onde ocorre o JOIN na consulta
                    .AsNoTracking()
                    .Where(x => x.CategoryId == id) //Filtro
                    .ToListAsync();

            return Ok(products);
        }

        [HttpPost]
        [Route("")]
        [Authorize]
        public async Task<ActionResult<List<Product>>> Post( 
                [FromBody]Product product,
                [FromServices] DataContext context){
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                context.Products.Add(product); // gera um id automático
                await context.SaveChangesAsync();
                return Ok(product);
            }
            catch 
            {
                return BadRequest( new { message = "Não foi possível adicionar esse produto"});
            }

            
        }

        [HttpPut]
        [Route("{id:int}")]
        [Authorize] //Necessidade de estar autenticado para acessar o método
        public async Task<ActionResult<List<Product>>> Put(
            int id, 
            [FromBody]Product product,
            [FromServices]DataContext context
        )
            {
            if(id != product.Id)
                return NotFound( new { message = "Categoria não encontrada"});
            
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                context.Entry<Product>(product).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                await context.SaveChangesAsync();
                return Ok(product);
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest(new { message = "Este registro já foi atualizado"});
            }
            catch(Exception){
                return BadRequest(new { message = "Não foi possível atualizar o produto"});
            }
        }

        [HttpDelete]
        [Route("{id:int}")]
        [Authorize(Roles = "employee")] //Necessidade de estar autenticado e de ser o employee para acessar o método
        async Task<ActionResult<List<Product>>> Delete(int id, [FromServices]DataContext context){
            var product = await context.Products.FirstOrDefaultAsync(x => x.Id == id);
            // FirstOrDefaultAsync -> retorna o primeiro registro encontrado se forem encontrados mais de um e um null se nada for encontrado

            if(product == null)
                return NotFound(new { message = "Produto não encontrada"});

            try
            {
                context.Products.Remove(product);
                await context.SaveChangesAsync();
                return Ok(new {message = "Produto removido com sucesso!"});
            }
            catch (System.Exception)
            {
                return BadRequest(new {message = "Não foi possível excluir o produto informado"});
            }
        }


    }
}