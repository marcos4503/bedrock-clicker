using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Bedrock_Clicker
{
    /// <summary>
    /// Lógica interna para WindowPvpTips.xaml
    /// </summary>
    public partial class WindowPvpTips : Window
    {
        public WindowPvpTips()
        {
            InitializeComponent();
            OnStart();
        }

        //Window methods

        public void OnStart()
        {
            //Inflate with english by default
            InflateEnglish();

            //Add event to change language
            language.SelectionChanged += (sender, eventt) =>
            {
                if(language.SelectedIndex == 0)
                    InflateEnglish();
                if (language.SelectedIndex == 1)
                    InflatePortuguese();
            };
        }

        public void InflateEnglish()
        {
            //Set title
            title.Content = "PVP Tips!";

            //Tip #1
            tip1.Text = "First, practice your aim. Good aim, even during PVP movementation, is the key to success, even more when using the Bedrock Clicker!";
            //Tip #2
            tip2.Text = "Adjust your mouse's DPI to a value that's comfortable for you, which makes it easier to improve your aim!";
            //Tip #3
            tip3.Text = "Use the Field of View at 75. A smaller field of view will distort your view of the game less, which can help during PVP.";
            //Tip #4
            tip4.Text = "Don't jump around during PVP. The Bedrock Clicker will already have you clicking fast enough that you only have to worry about aiming and other tactics. In addition to jumping making you more vulnerable to Knockbacks and such, the Crit caused by jumping is disabled on many servers.";
            //Tip #5
            tip5.Text = "Move in a circular shape around your opponent, and in random directions left or right. This can confuse your opponent and also make him miss some clicks.";
            //Tip #6
            tip6.Text = "If you are at the same height as your opponent, always aim at Head or Shoulder height. If your opponent is at a higher height than you, aim for the Legs.";
            //Tip #7
            tip7.Text = "Be unpredictable during combat. Surprise your opponent, avoid doing things that are predictable, like ALWAYS go for him at all costs and run. Also, try your best to make life difficult for your opponent, for example, getting on top, starting combos with arrows, having plans to hit while he can't hit you, etc.";
            //Tip #8
            tip8.Text = "Enemies that are in the air, or above you, take more Knockback and have a harder time hitting you. So, take advantage of this and do your best to keep hitting.";
            //Tip #9
            tip9.Text = "During combat, press W repeatedly while continuing to hit as this will throw the enemy further away and keep you closer to him. Try to stay out of his sword range though, if necessary, press S to walk back if you feel you've come within his range.";
            //Tip #10
            tip10.Text = "The player who is on a lower floor always has an advantage since in Minecraft Bedrock the range of the sword is counted from the Head, so the enemy who is above often does not have enough range to hit because counting from his Head to you, the distance can be great so that only you can hit.";
            //Tip #11
            tip11.Text = "If you are caught in a sequence of hits from the enemy, and you are being thrown into the air, use a Bow, Eggs, Snowballs or place blocks between you and him, to stop the sequence of hits instantly.";
        }

        public void InflatePortuguese()
        {
            //Set title
            title.Content = "Dicas Para PVP!";

            //Tip #1
            tip1.Text = "Primeiramente, pratique sua mira. Uma mira boa, mesmo durante a movimentação do PVP, é a chave para o sucesso, ainda mais usando o Bedrock Clicker!";
            //Tip #2
            tip2.Text = "Ajuste o DPI do seu mouse para um valor confortável para você, que facilite para melhorar sua mira!";
            //Tip #3
            tip3.Text = "Utilize o Campo de Visão em 75. Um campo de visão menor distorcerá menos a sua visão do jogo, o que pode ajudar durante o PVP.";
            //Tip #4
            tip4.Text = "Não fique pulando durante o PVP. O Bedrock Clicker já te deixará clicando rápido o suficiente para que se preocupe apenas com a mira e outras táticas. Além do pulo te deixar mais vulnerável a Knockbacks e outras coisas, o Crítico causado por pulos é desativado em muitos servidores.";
            //Tip #5
            tip5.Text = "Movimente-se num formato circular ao redor do seu oponente, e para direções aleatórias na esquerda ou direita. Isso poderá confundir seu oponente e também faze-lo errar alguns clicks.";
            //Tip #6
            tip6.Text = "Se estiver na mesma altura do seu oponente, sempre mire na altura da Cabeça ou Ombro dele. Se o seu oponente estiver numa altura maior que a sua, mire nas Pernas.";
            //Tip #7
            tip7.Text = "Seja imprevisível durante o combate. Surpreenda seu oponente, evite fazer coisas que são previsíveis, como SEMPRE ir para cima dele a todo custo e correndo. Além disso, tente ao máximo dificultar a vida do seu oponente, como por exemplo, chegar por cima, começar combo com flechas, ter planos para conseguir bater enquanto ele não pode te bater e etc.";
            //Tip #8
            tip8.Text = "Inimigos que estão no ar, ou acima de você, tomam mais Knockback e tem mais dificuldade para bater em você. Então, aproveite isso e faça o possível para continuar batendo.";
            //Tip #9
            tip9.Text = "Durante o combate, pressione o W repetidamente enquanto continua batendo pois isso irá jogar o inimigo mais longe e te manter mais proximo dele. Entretanto tente se manter fora do alcance da espada dele, se necessário, pressione S para andar pra tras se notar que entrou dentro do alcance dele.";
            //Tip #10
            tip10.Text = "O jogador que esta num piso mais baixo sempre tem vantagem já que no Minecraft Bedrock o alcance da espada é contado a partir da Cabeça, então o inimigo que esta acima muitas vezes não consegue ter alcance o suficiente para bater pois contando da Cabeça dele até você, a distancia pode ser grande fazendo com que só você consiga bater.";
            //Tip #11
            tip11.Text = "Se você está preso numa sequência de golpes do inimigo, e esta sendo jogado para o alto, utilize um Arco, Ovos, Bola de Neve ou coloque blocos entre você e ele, para parar a sequencia de golpes instantaneamente.";
        }
    }
}
