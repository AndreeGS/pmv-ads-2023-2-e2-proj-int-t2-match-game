using Match_Game_Oficial.Models;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Http;

namespace Match_Game_Oficial.Controllers
{
    public class EsqsenhaController : Controller
    {
        private const string CodigoDeVerificacaoChave = "CodigoDeVerificacao"; // Chave para armazenar o c�digo na sess�o

        // A��o para solicitar o email
        public IActionResult SolicitarEmail()
        {
            return View();
        }

        // A��o para enviar o email com o c�digo de verifica��o
        [HttpPost]
        public IActionResult EnviarEmail(Esqsenha model)
        {
            if (ModelState.IsValid)
            {
                // Gere um c�digo de verifica��o
                string codigoDeVerificacao = GerarCodigoDeVerificacao();

                // Envie o email com o c�digo
                EnviarEmailComCodigo(model.Email, codigoDeVerificacao);

                // Armazene o c�digo de verifica��o na sessao
                HttpContext.Session.SetString(CodigoDeVerificacaoChave, codigoDeVerificacao);

                // Redirecione para a p�gina de inser��o do c�digo
                return RedirectToAction("InserirCodigo");
            }

            return View("SolicitarEmail", model);
        }

        private string GerarCodigoDeVerificacao()
        {
            // L�gica para gerar um c�digo de verificacao
            return "123456"; // Substitua isso pela l�gica real
        }

        private void EnviarEmailComCodigo(string email, string codigo)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Seu Nome", "seuemail@example.com"));
            message.To.Add(new MailboxAddress("Nome do Destinat�rio", email));
            message.Subject = "C�digo de Verifica��o";

            var text = new TextPart("plain")
            {
                Text = $"Seu c�digo de verifica��o: {codigo}"
            };

            message.Body = text;

            using (var client = new SmtpClient())
            {
                client.Connect("smtp.example.com", 587, false); // Substitua com as informa��es do seu servidor SMTP
                client.Authenticate("seuemail@example.com", "suasenha"); // Substitua com suas credenciais de email
                client.Send(message);
                client.Disconnect(true);
            }
        }

        // A��o para inserir o c�digo de verifica��o
        public IActionResult InserirCodigo()
        {
            return View();
        }

        // A��o para redefinir a senha
        [HttpPost]
        public IActionResult RedefinirSenha(Esqsenha model)
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

                // Implemente a l�gica para redefinir a senha do usu�rio
                // Substitua o exemplo abaixo pela l�gica real
                // Exemplo: Atualizar a senha no banco de dados

                // Limpe o c�digo de verificacao da sessao
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
    }

}


