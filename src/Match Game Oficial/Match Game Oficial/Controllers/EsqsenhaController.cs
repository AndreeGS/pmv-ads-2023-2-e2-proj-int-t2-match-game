using Match_Game_Oficial.Models;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net;


namespace Match_Game_Oficial.Controllers
{
    public class EsqsenhaController : Controller
    {
        private readonly DataContext _context;

        private readonly SmtpSettings _smtpSettings;

        private const string CodigoDeVerificacaoChave = "CodigoDeVerificacao"; // Chave para armazenar o c�digo na sess�o


      

        //COnfigura��o do SMTP
        public EsqsenhaController(IOptions<SmtpSettings> smtpSettings, DataContext context)
        {
            _smtpSettings = smtpSettings.Value;

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

                // Chamada do m�todo para enviar o email com o c�digo
                EnviarEmailComCodigo(email, codigoDeVerificacao, _smtpSettings);

                // Armazene o c�digo de verifica��o na sess�o
                HttpContext.Session.SetString(CodigoDeVerificacaoChave, codigoDeVerificacao);

                // Redirecionamento para a p�gina de inser��o do c�digo
                return View("InserirCodigo");
            }
        }

        //M�todo para gerar um c�digo aleat�rio
        private string GerarCodigoDeVerificacao()
        {
            Random random = new();
            int codigo = random.Next(10000, 100000);
            
            return codigo.ToString("D5"); 
        }

        //M�todo que envia um email para o usu�rio com o SMTP
         private async Task EnviarEmailComCodigo(string email, string codigo, SmtpSettings smtpSettings)
        {
            //Aqui � onde ocorre a montagem do email
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Match Game", smtpSettings.SmtpUsername));
            message.To.Add(new MailboxAddress("Nome do Destinat�rio", email));
            message.Subject = "C�digo de Verifica��o";

            var text = new TextPart("plain")
            {
                Text = $"Seu c�digo de verifica��o: {codigo}"
            };

            message.Body = text;

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(smtpSettings.SmtpServer, smtpSettings.SmtpPort, true); // Usando configura��es do appsettings.json
                await client.AuthenticateAsync(smtpSettings.SmtpUsername, smtpSettings.SmtpPassword); // Usando configura��es do appsettings.json
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }


        // A��o para inserir o c�digo de verifica��o
        public IActionResult InserirCodigo()
        {
            return View();
        }

        // A��o para redefinir a senha
        [HttpPost]
        public async Task <IActionResult> RedefinirSenha(Esqsenha model)
        {
            if (ModelState.IsValid)
            {
                // Verifique o c�digo de verifica��o na sess�o
                string codigoDeVerificacao = HttpContext.Session.GetString(CodigoDeVerificacaoChave);

                if (model.Codigo != codigoDeVerificacao)
                {
                    ModelState.AddModelError("Codigo", "C�digo de verifica��o incorreto");
                    return View(model);
                }

                //Aqui, estamos resgatando o usu�rio que tenha o email fornecido
                var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == model.Email);

                //Definindo a nova senha
                usuario.Senha = model.NovaSenha;

                //Enviando para o Bando de Dados
                _context.Update(usuario);
                await _context.SaveChangesAsync();


                // Limpando o c�digo de verificacao da sessao
                HttpContext.Session.Remove(CodigoDeVerificacaoChave);

                // Redirecione para a p�gina de confirma��o de senha
                return RedirectToAction("ConfirmacaoSenha");
            }

            return View(model);
        }

        // A��o para a p�gina de confirma��o de senha
        public IActionResult ConfirmacaoSenha()
        {
            return View();
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


