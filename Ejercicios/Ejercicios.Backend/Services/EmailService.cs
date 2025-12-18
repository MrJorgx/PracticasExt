using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace Ejercicios.Backend.Services
{
    public interface IEmailService
    {
        Task SendTwoFactorCodeAsync(string toEmail, string toName, string code);
        Task SendWelcomeEmailAsync(string toEmail, string toName);
        Task SendPasswordResetEmailAsync(string toEmail, string toName, string resetToken);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendTwoFactorCodeAsync(string toEmail, string toName, string code)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress("Ejercicios App", _configuration["Email:From"]));
                email.To.Add(new MailboxAddress(toName, toEmail));
                email.Subject = "Código de verificación - Ejercicios App";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                        <!DOCTYPE html>
                        <html>
                        <head>
                            <style>
                                body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                                .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                                .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
                                .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
                                .code-box {{ background: white; border: 2px dashed #667eea; padding: 20px; text-align: center; margin: 20px 0; border-radius: 8px; }}
                                .code {{ font-size: 32px; font-weight: bold; color: #667eea; letter-spacing: 8px; }}
                                .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
                                .warning {{ background: #fff3cd; border-left: 4px solid #ffc107; padding: 10px; margin: 20px 0; }}
                            </style>
                        </head>
                        <body>
                            <div class='container'>
                                <div class='header'>
                                    <h1>🔐 Código de Verificación</h1>
                                </div>
                                <div class='content'>
                                    <p>Hola <strong>{toName}</strong>,</p>
                                    <p>Has solicitado iniciar sesión en tu cuenta de Ejercicios App. Para completar el proceso, utiliza el siguiente código de verificación:</p>
                                    
                                    <div class='code-box'>
                                        <div class='code'>{code}</div>
                                    </div>
                                    
                                    <div class='warning'>
                                        <strong>⚠️ Importante:</strong>
                                        <ul>
                                            <li>Este código expira en <strong>10 minutos</strong></li>
                                            <li>Nunca compartas este código con nadie</li>
                                            <li>Si no solicitaste este código, ignora este email</li>
                                        </ul>
                                    </div>
                                    
                                    <p>Si no has solicitado este código, por favor ignora este mensaje. Tu cuenta permanece segura.</p>
                                </div>
                                <div class='footer'>
                                    <p>Este es un email automático, por favor no respondas.</p>
                                    <p>&copy; 2024 Ejercicios App. Todos los derechos reservados.</p>
                                </div>
                            </div>
                        </body>
                        </html>"
                };

                email.Body = bodyBuilder.ToMessageBody();

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(
                    _configuration["Email:Host"],
                    int.Parse(_configuration["Email:Port"] ?? "587"),
                    SecureSocketOptions.StartTls
                );

                await smtp.AuthenticateAsync(
                    _configuration["Email:Username"],
                    _configuration["Email:Password"]
                );

                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation("Código 2FA enviado exitosamente a {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando código 2FA a {Email}", toEmail);
                throw;
            }
        }

        public async Task SendWelcomeEmailAsync(string toEmail, string toName)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress("Ejercicios App", _configuration["Email:From"]));
                email.To.Add(new MailboxAddress(toName, toEmail));
                email.Subject = "¡Bienvenido a Ejercicios App!";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                        <!DOCTYPE html>
                        <html>
                        <head>
                            <style>
                                body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                                .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                                .header {{ background: linear-gradient(135deg, #28a745 0%, #20c997 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
                                .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
                                .button {{ display: inline-block; background: #667eea; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
                                .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
                            </style>
                        </head>
                        <body>
                            <div class='container'>
                                <div class='header'>
                                    <h1>🎉 ¡Bienvenido!</h1>
                                </div>
                                <div class='content'>
                                    <p>Hola <strong>{toName}</strong>,</p>
                                    <p>¡Gracias por registrarte en Ejercicios App! Tu cuenta ha sido creada exitosamente.</p>
                                    
                                    <p>Ahora puedes:</p>
                                    <ul>
                                        <li>✅ Acceder a 7 ejercicios interactivos</li>
                                        <li>✅ Practicar algoritmos y estructuras de datos</li>
                                        <li>✅ Gestionar tu perfil personalizado</li>
                                        <li>✅ Habilitar autenticación de dos factores para mayor seguridad</li>
                                    </ul>
                                    
                                    <div style='text-align: center;'>
                                        <a href='http://localhost:5088' class='button'>Ir a la aplicación</a>
                                    </div>
                                    
                                    <p>Si tienes alguna pregunta, no dudes en contactarnos.</p>
                                    
                                    <p>¡Disfruta de la experiencia!</p>
                                </div>
                                <div class='footer'>
                                    <p>&copy; 2024 Ejercicios App. Todos los derechos reservados.</p>
                                </div>
                            </div>
                        </body>
                        </html>"
                };

                email.Body = bodyBuilder.ToMessageBody();

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(
                    _configuration["Email:Host"],
                    int.Parse(_configuration["Email:Port"] ?? "587"),
                    SecureSocketOptions.StartTls
                );

                await smtp.AuthenticateAsync(
                    _configuration["Email:Username"],
                    _configuration["Email:Password"]
                );

                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation("Email de bienvenida enviado a {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando email de bienvenida a {Email}", toEmail);
            }
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string toName, string resetToken)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress("Ejercicios App", _configuration["Email:From"]));
                email.To.Add(new MailboxAddress(toName, toEmail));
                email.Subject = "Recuperación de contraseña - Ejercicios App";

                // URL del frontend para resetear contraseña
                var resetUrl = $"http://localhost:5088/reset-password?token={resetToken}&email={Uri.EscapeDataString(toEmail)}";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = $@"
                        <!DOCTYPE html>
                        <html>
                        <head>
                            <style>
                                body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                                .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                                .header {{ background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
                                .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
                                .button {{ display: inline-block; background: #667eea; color: white; padding: 15px 30px; text-decoration: none; border-radius: 5px; margin: 20px 0; font-weight: bold; }}
                                .button:hover {{ background: #5568d3; }}
                                .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
                                .warning {{ background: #fff3cd; border-left: 4px solid #ffc107; padding: 10px; margin: 20px 0; }}
                                .token-box {{ background: white; border: 2px dashed #667eea; padding: 15px; text-align: center; margin: 20px 0; border-radius: 8px; }}
                                .token {{ font-size: 18px; font-weight: bold; color: #667eea; word-break: break-all; }}
                            </style>
                        </head>
                        <body>
                            <div class='container'>
                                <div class='header'>
                                    <h1>🔑 Recuperación de Contraseña</h1>
                                </div>
                                <div class='content'>
                                    <p>Hola <strong>{toName}</strong>,</p>
                                    <p>Hemos recibido una solicitud para restablecer la contraseña de tu cuenta en Ejercicios App.</p>
                                    
                                    <p>Haz clic en el siguiente botón para crear una nueva contraseña:</p>
                                    
                                    <div style='text-align: center;'>
                                        <a href='{resetUrl}' class='button'>Restablecer Contraseña</a>
                                    </div>
                                    
                                    <p>O copia y pega el siguiente enlace en tu navegador:</p>
                                    
                                    <div class='token-box'>
                                        <div class='token'>{resetUrl}</div>
                                    </div>
                                    
                                    <div class='warning'>
                                        <strong>⚠️ Importante:</strong>
                                        <ul>
                                            <li>Este enlace expira en <strong>1 hora</strong></li>
                                            <li>Solo puedes usar este enlace una vez</li>
                                            <li>Si no solicitaste este cambio, ignora este email</li>
                                            <li>Tu contraseña no cambiará hasta que accedas al enlace y establezcas una nueva</li>
                                        </ul>
                                    </div>
                                    
                                    <p>Si no solicitaste restablecer tu contraseña, puedes ignorar este mensaje de forma segura.</p>
                                </div>
                                <div class='footer'>
                                    <p>Este es un email automático, por favor no respondas.</p>
                                    <p>&copy; 2024 Ejercicios App. Todos los derechos reservados.</p>
                                </div>
                            </div>
                        </body>
                        </html>"
                };

                email.Body = bodyBuilder.ToMessageBody();

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(
                    _configuration["Email:Host"],
                    int.Parse(_configuration["Email:Port"] ?? "587"),
                    SecureSocketOptions.StartTls
                );

                await smtp.AuthenticateAsync(
                    _configuration["Email:Username"],
                    _configuration["Email:Password"]
                );

                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation("Email de recuperación de contraseña enviado a {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando email de recuperación de contraseña a {Email}", toEmail);
                throw;
            }
        }
    }
}