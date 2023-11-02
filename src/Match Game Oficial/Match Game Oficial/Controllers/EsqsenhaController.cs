using Match_Game_Oficial.Models;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
//using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading;
using System.ComponentModel;
using System.Net;
using Org.BouncyCastle.Crypto.Macs;

namespace Match_Game_Oficial.Controllers
{
    public class EsqsenhaController : Controller
    {
        private readonly DataContext _context;

     

        private const string CodigoDeVerificacaoChave = "CodigoDeVerificacao"; // Chave para armazenar o c�digo na sess�o


      

        //COnfigura��o do SMTP
        public EsqsenhaController( DataContext context)
        {
            

            _context = context;
        }
        // A��o para solicitar o email
        public IActionResult SolicitarEmail()
        {
            return View();
        }

        // A��o para enviar o email com o c�digo de verifica��o
        [HttpPost]
        public async Task<IActionResult> EnviarEmail(Esqsenha model)
        {
            string email = HttpContext.Request.Form["Email"];

            // Valide o email
            bool emailIsValid = await IsValidEmail(email);

            if (!emailIsValid)
            {
                ModelState.AddModelError("Email", "O endere�o de email n�o � v�lido.");
                return View("SolicitarEmail");
            }
            else
            {
                // Chamada do m�todo para gerar c�digo de verifica��o
                string codigoDeVerificacao = GerarCodigoDeVerificacao();

                Console.WriteLine(codigoDeVerificacao);


                // Chamada do m�todo para enviar o email com o c�digo
                EnviarEmailComCodigo(email, codigoDeVerificacao);
                Console.WriteLine(email);


                // Armazene o c�digo de verifica��o na sess�o
                HttpContext.Session.SetString(CodigoDeVerificacaoChave, codigoDeVerificacao);

                // Redirecionamento para a p�gina de inser��o do c�digo
                return View("InserirCodigo");
            }
        }

        //M�todo para gerar um c�digo aleat�rio
        private string GerarCodigoDeVerificacao()
        {
            Random random = new Random();
            int codigo = random.Next(10000, 100000);
            
            return codigo.ToString("D5"); 
        }

        //M�todo que envia um email para o usu�rio com o SMTP
        private void EnviarEmailComCodigo(string email, string codigoDeVerificacao)
        {
            string fromMail = "matchgame02@gmail.com";
            string fromPassword = "ynzupkoqpimoynbh";

            MailMessage message = new MailMessage();
            message.From = new MailAddress(fromMail);

            message.Subject = "C�digo de Verifica��o";
            message.To.Add(new MailAddress(email));
            message.Body = $"<p>Seu c�digo �: {codigoDeVerificacao}<p>";
            message.IsBodyHtml = true;

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromMail, fromPassword),
                EnableSsl = true,
            };

            smtpClient.Send(message);
        }


        // A��o para inserir o c�digo de verifica��o
        public IActionResult InserirCodigo()
        {
            return View();
        }

        [HttpPost]
        public async Task VerificaCodigo(Esqsenha model)
        {
            string codigo = HttpContext.Request.Form["Codigo"];


            if (codigo == HttpContext.Session.GetString(CodigoDeVerificacaoChave))
            {
                Console.WriteLine("Seu codigo est� correto");
            }
            else
            {
                Console.WriteLine("Esta errado");
            }

        }

        


//M�todo que v�lida se o email existe no Banco de Dados
public async Task<bool> IsValidEmail(string email)
        {
            var emailUser = await _context.Usuarios
                .Where(u => u.Email == email)
                .FirstOrDefaultAsync();

            return emailUser != null;
        }


    }


}


