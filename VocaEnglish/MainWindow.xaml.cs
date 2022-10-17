using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using CSCore;
using CSCore.Codecs;
using CSCore.CoreAudioAPI;
using CSCore.SoundIn;
using CSCore.SoundOut;
using CSCore.Streams.Effects;
using CSCore.Streams;
using System.Threading;

namespace VocaEnglish
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private FileInfo fileInfo = new FileInfo("window.tmp");
        private FileInfo fileInfo1 = new FileInfo("Data_Load.tmp");
        private FileInfo FileLanguage = new FileInfo("Data_Language.tmp");
        private FileInfo fileinfo = new FileInfo("DataTemp.tmp");

        private MMDeviceCollection mOutputDevices;
        private MMDeviceCollection mInputDevices;
        private WasapiOut mSoundOut;
        private WasapiCapture mSoundIn;
        private ISampleSource mMp3;
        private IWaveSource mSource;

        private SimpleMixer mMixer;
        private SampleDSPPitch mDspPitch;

        private float pitchVal;
        private float reverbVal;
        private int SampleRate, SoundClick = 0, ImgBtnTurboClick = 0;

        string langindex;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void SoundIn()
        {
            try
            {
                mSoundIn = new WasapiCapture(/*false, AudioClientShareMode.Exclusive, 1*/);
                Dispatcher.Invoke(() => mSoundIn.Device = mInputDevices[cmbInput.SelectedIndex]);
                mSoundIn.Initialize();
                mSoundIn.Start();
            }
            catch (Exception ex)
            {
                if (langindex == "0")
                {
                    string msg = "Ошибка в SoundIn: \r\n" + ex.Message;
                    LogClass.LogWrite(msg);
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
                else
                {
                    string msg = "Error in SoundIn: \r\n" + ex.Message;
                    LogClass.LogWrite(msg);
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
            }
        }

        private void SoundOut()
        {
            try
            {

                mSoundOut = new WasapiOut(/*false, AudioClientShareMode.Exclusive, 1*/);
                Dispatcher.Invoke(() => mSoundOut.Device = mOutputDevices[cmbOutput.SelectedIndex]);
                //mSoundOut.Device = mOutputDevices[cmbOutput.SelectedIndex];


                if (SoundClick == 0)
                {
                    mSoundOut.Initialize(mMp3.ToWaveSource(32).ToMono());
                    mSoundOut.Volume = 5;
                }
                else
                {
                    mSoundOut.Initialize(mMixer.ToWaveSource(32).ToMono());
                    mSoundOut.Volume = 15;
                }


                mSoundOut.Play();


            }
            catch (Exception ex)
            {
                if (langindex == "0")
                {
                    string msg = "Ошибка в SoundOut: \r\n" + ex.Message;
                    LogClass.LogWrite(msg);
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);

                }
                else
                {
                    string msg = "Error in SoundOut: \r\n" + ex.Message;
                    LogClass.LogWrite(msg);
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);

                }
            }
        }

        private async void Sound(string FileName)
        {
            try
            {
                SoundClick = 0;
                Mixer();
                mMp3 = CodecFactory.Instance.GetCodec(FileName).ToMono().ToSampleSource();
                mMixer.AddSource(mMp3.ChangeSampleRate(mMp3.WaveFormat.SampleRate));
                await Task.Run(() => SoundOut());
            }
            catch (Exception ex)
            {
                if (langindex == "0")
                {
                    string msg = "Ошибка в Sound: \r\n" + ex.Message;
                    LogClass.LogWrite(msg);
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
                else
                {
                    string msg = "Error in Sound: \r\n" + ex.Message;
                    LogClass.LogWrite(msg);
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
            }
        }

        private async void StartFullDuplex()//запуск пича и громкости
        {
            try
            {
                SoundClick = 1;
                //Запускает устройство захвата звука с задержкой 1 мс.
                //await Task.Run(() => SoundIn());
                SoundIn();

                var source = new SoundInSource(mSoundIn) { FillWithZeros = true };
                var xsource = source.ToSampleSource();

                var reverb = new DmoWavesReverbEffect(xsource.ToWaveSource());
                reverb.ReverbTime = reverbVal;
                reverb.HighFrequencyRTRatio = ((float)1) / 1000;
                xsource = reverb.ToSampleSource();

                //Init DSP для смещения высоты тона
                mDspPitch = new SampleDSPPitch(xsource.ToMono());

                SetPitchShiftValue();

                //Инициальный микшер
                Mixer();

                //Добавляем наш источник звука в микшер
                mMixer.AddSource(mDspPitch.ChangeSampleRate(mMixer.WaveFormat.SampleRate));

                //Запускает устройство воспроизведения звука с задержкой 1 мс.
                await Task.Run(() => SoundOut());

            }
            catch (Exception ex)
            {
                if (langindex == "0")
                {
                    string msg = "Ошибка в StartFullDuplex: \r\n" + ex.Message;
                    LogClass.LogWrite(msg);
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
                else
                {
                    string msg = "Error in StartFullDuplex: \r\n" + ex.Message;
                    LogClass.LogWrite(msg);
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
            }
            //return false;
        }

        private async void StartFullDuplex1()//запуск пича и громкости
        {
            try
            {
                SoundClick = 1;
                //Запускает устройство захвата звука с задержкой 1 мс.
                //await Task.Run(() => SoundIn());
                SoundIn();

                var source = new SoundInSource(mSoundIn) { FillWithZeros = true };

                //Init DSP для смещения высоты тона
                mDspPitch = new SampleDSPPitch(source.ToSampleSource().ToMono());

                SetPitchShiftValue();

                //Инициальный микшер
                Mixer();

                //Добавляем наш источник звука в микшер
                mMixer.AddSource(mDspPitch.ChangeSampleRate(mMixer.WaveFormat.SampleRate));

                //Запускает устройство воспроизведения звука с задержкой 1 мс.
                await Task.Run(() => SoundOut());

            }
            catch (Exception ex)
            {
                if (langindex == "0")
                {
                    string msg = "Ошибка в StartFullDuplex: \r\n" + ex.Message;
                    LogClass.LogWrite(msg);
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
                else
                {
                    string msg = "Error in StartFullDuplex: \r\n" + ex.Message;
                    LogClass.LogWrite(msg);
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
            }
            //return false;
        }

        private void SetPitchShiftValue()
        {
            mDspPitch.PitchShift = (float)Math.Pow(2.0F, pitchVal / 13.0F);
        }

        private void Feeling_in_the_body_pattern()
        {
            TembroClass tembro = new TembroClass();
            string PathFile = @"VocaEnglish\Pattern\Wide_voiceMan.tmp";
            tembro.Tembro(SampleRate, PathFile);
            pitchVal = 0;
            reverbVal = 150;
        }

        private void TimerRec()
        {
            Dispatcher.Invoke(() => lbTimer.Visibility = Visibility.Visible);
            int i = 3;
            while (i > 0)
            {
                Dispatcher.Invoke(() => lbTimer.Content = i.ToString());
                Thread.Sleep(1000);
                i--;
            }
            Dispatcher.Invoke(() => lbTimer.Content = i.ToString());
            Dispatcher.Invoke(() => lbTimer.Visibility = Visibility.Hidden);
        }

        private async void EnglishVoca()
        {
            Feeling_in_the_body_pattern();

            btnPlay.Visibility = Visibility.Hidden;

            /*lbText.Content = "Сейчас начнется трёх минутная подготовка";
            lbText.Visibility = Visibility.Visible;
            await Task.Delay(3000);
            lbText.Visibility = Visibility.Hidden; 

            Stop();
            StartFullDuplex();
            await Task.Run(() => PitchTimerFeelInTheBody());
            Stop();*/

            lbText.Content = "Хорошо. Сейчас начнут появляться слова на экране\nслушайте их, и повторняйте их, после фразы 'ПОВТОРИТЕ'";
            lbText.Visibility = Visibility.Visible;
            await Task.Delay(6000);

            lbText.Content = "hint\n[hɪnt]\nнамек, совет, оттенок";
            Sound(@"VocaEnglish\Words\hint.wav");
            await Task.Delay(2000);
            Stop();

            /*(lbSubText.Visibility = Visibility.Visible;
            lbSubText.Content = "ПОВТОРИТЕ";
            await Task.Run(() => TimerRec());
            lbSubText.Visibility = Visibility.Hidden;*/
            await Task.Run(() => TimeTextRep());

            //На всякий случай мало ли что

            Sound(@"VocaEnglish\Words\hint.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            lbText.Content = "ferry\n[ˈfer.i]\nпаром, переправа";
            Sound(@"VocaEnglish\Words\ferry.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            Sound(@"VocaEnglish\Words\ferry.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            lbText.Content = "dairy\n[ˈdeə.ri]\nмолочная, молочный, маслодельня";
            Sound(@"VocaEnglish\Words\dairy.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            Sound(@"VocaEnglish\Words\dairy.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            lbText.Content = "plank\n[plæŋk]\nпланка, доска, обшивная доска или планка";
            Sound(@"VocaEnglish\Words\plank.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            Sound(@"VocaEnglish\Words\plank.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            lbText.Content = "wag\n[wæɡ]\n/шутник, взмах, бездельник";
            Sound(@"VocaEnglish\Words\wag.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            Sound(@"VocaEnglish\Words\wag.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            lbText.Content = "throttle\n[ˈθrɒt.l̩]\nдушить, дросселировать, задыхаться";
            Sound(@"VocaEnglish\Words\throttle.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            Sound(@"VocaEnglish\Words\throttle.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            lbText.Content = "stubble\n[ˈstʌb.əl]\nщетина";
            Sound(@"VocaEnglish\Words\stubble.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            Sound(@"VocaEnglish\Words\stubble.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            lbText.Content = "pittance\n[ˈpɪt.əns]\nжалкие гроши, подачка";
            Sound(@"VocaEnglish\Words\pittance.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            Sound(@"VocaEnglish\Words\pittance.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            lbText.Content = "midget\n[ˈmɪdʒ.ɪt]\nкарлик, лилипут";
            Sound(@"VocaEnglish\Words\midget.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            Sound(@"VocaEnglish\Words\midget.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            lbText.Content = "botch\n[bɒtʃ]\nпортить, неумело латать, делать небрежно";
            Sound(@"VocaEnglish\Words\botch.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            Sound(@"VocaEnglish\Words\botch.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            lbText.Content = "prig\n[prɪɡ]\nпедант, формалист";
            Sound(@"VocaEnglish\Words\prig.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            Sound(@"VocaEnglish\Words\prig.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            lbText.Content = "soothsayer\n[ˈsuːθˌseɪ.ər]\nпредсказатель, гадалка";
            Sound(@"VocaEnglish\Words\soothsayer.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            Sound(@"VocaEnglish\Words\soothsayer.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            lbText.Content = "imbroglio\n[ɪmˈbrəʊ.li.əʊ]\nпутаница, запутанная ситуация, сложная ситуация";
            Sound(@"VocaEnglish\Words\imbroglio.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            Sound(@"VocaEnglish\Words\imbroglio.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            lbText.Content = "layer\n[ˈleɪə(r)]\nслой, уровень, пласт";
            Sound(@"VocaEnglish\Words\layer.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            Sound(@"VocaEnglish\Words\layer.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            lbText.Content = "delve\n[delv]\nкопаться, рыться, копать";
            Sound(@"VocaEnglish\Words\delve.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            Sound(@"VocaEnglish\Words\delve.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            lbText.Content = "disquisition\n[ˌdɪs.kwɪˈzɪʃ.ən]\nисследование, дознание, подробное исследование";
            Sound(@"VocaEnglish\Words\disquisition.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            Sound(@"VocaEnglish\Words\disquisition.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            lbText.Content = "winnow\n[ˈwɪn.əʊ]\nвеять, отсеивать";
            Sound(@"VocaEnglish\Words\winnow.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            Sound(@"VocaEnglish\Words\winnow.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            lbText.Content = "ugly\n[ˈʌɡli]\nуродливый, безобразный, неприятный";
            Sound(@"VocaEnglish\Words\ugly.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            Sound(@"VocaEnglish\Words\ugly.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            lbText.Content = "guard\n[ɡɑːd]\nстража, охрана, гвардия";
            Sound(@"VocaEnglish\Words\guard.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            Sound(@"VocaEnglish\Words\guard.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            lbText.Content = "dust\n[dʌst]\nпыль, прах, пыльца";
            Sound(@"VocaEnglish\Words\dust.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            Sound(@"VocaEnglish\Words\dust.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            lbText.Content = "interdependent\n[ˌɪn.tə.dɪˈpen.dənt]\nвзаимозависимый, зависящий один от другого";
            Sound(@"VocaEnglish\Words\interdependent.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            Sound(@"VocaEnglish\Words\interdependent.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            lbText.Content = "commonality\n[ˌkɒm.ənˈæl.ə.ti]\nобщность";
            Sound(@"VocaEnglish\Words\commonality.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            Sound(@"VocaEnglish\Words\commonality.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            lbText.Content = "overreach\n[ˌəʊ.vəˈriːtʃ]\nперехитрить, овладевать";
            Sound(@"VocaEnglish\Words\overreach.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            Sound(@"VocaEnglish\Words\overreach.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            lbText.Content = "moth\n[mɒθ]\nмоль, мотылек, ночная бабочка";
            Sound(@"VocaEnglish\Words\moth.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            Sound(@"VocaEnglish\Words\moth.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            lbText.Content = "tortoise\n[ˈtɔː.təs]\nчерепаха";
            Sound(@"VocaEnglish\Words\tortoise.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            Sound(@"VocaEnglish\Words\tortoise.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            lbText.Content = "pleasingly\n[ˈpliː.zɪŋ]\nприятно";
            Sound(@"VocaEnglish\Words\pleasingly.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            Sound(@"VocaEnglish\Words\pleasingly.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            lbText.Content = "adjustable\n[əˈdʒʌs.tə.bl̩]\nрегулируемый, разводной, устанавливаемый";
            Sound(@"VocaEnglish\Words\adjustable.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            Sound(@"VocaEnglish\Words\adjustable.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            lbText.Content = "depose\n[dɪˈpəʊz]\nсмещать, свергать";
            Sound(@"VocaEnglish\Words\depose.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            Sound(@"VocaEnglish\Words\depose.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            lbText.Content = "mitten\n[ˈmɪt.ən]\nрукавица, варежка";
            Sound(@"VocaEnglish\Words\mitten.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            Sound(@"VocaEnglish\Words\mitten.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            lbText.Content = "gambler\n[ˈɡæm.blər]\nигрок, картежник, азартный игрок";
            Sound(@"VocaEnglish\Words\gambler.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            Sound(@"VocaEnglish\Words\gambler.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            lbText.Content = "indecisive\n[ˌɪn.dɪˈsaɪ.sɪv]\nнерешительный, неокончательный, колеблющийся";
            Sound(@"VocaEnglish\Words\indecisive.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            Sound(@"VocaEnglish\Words\indecisive.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            lbText.Content = "revert\n[rɪˈvɜːt]\nвозвращаться, повернуть назад";
            Sound(@"VocaEnglish\Words\revert.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            Sound(@"VocaEnglish\Words\revert.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            lbText.Content = "ripen\n[ˈraɪ.pən]\nсозревать, дозреть, зреть";
            Sound(@"VocaEnglish\Words\ripen.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            Sound(@"VocaEnglish\Words\ripen.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            lbText.Content = "instigator\n[ˈɪn.stɪ.ɡeɪ.tər]\nподстрекатель, зачинщик";
            Sound(@"VocaEnglish\Words\instigator.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            Sound(@"VocaEnglish\Words\instigator.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            lbText.Content = "obstinacy\n[ˈɒb.stɪ.nət]\nупрямство, упорство, настойчивость";
            Sound(@"VocaEnglish\Words\obstinacy.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            Sound(@"VocaEnglish\Words\obstinacy.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            lbText.Content = "sodden\n[ˈsɒd.ən]\nпромокший, пропитанный, сырой";
            Sound(@"VocaEnglish\Words\sodden.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            Sound(@"VocaEnglish\Words\sodden.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            lbText.Content = "flabby\n[ˈflæb.i]\nдряблый, вялый, слабохарактерный";
            Sound(@"VocaEnglish\Words\flabby.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            Sound(@"VocaEnglish\Words\flabby.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            lbText.Content = "imbibe\n[ɪmˈbaɪb]\nвпитывать, поглощать, усваивать";
            Sound(@"VocaEnglish\Words\imbibe.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            Sound(@"VocaEnglish\Words\imbibe.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            lbText.Content = "grace\n[greɪs]\nблагодать";
            Sound(@"VocaEnglish\Words\grace.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            Sound(@"VocaEnglish\Words\grace.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            lbText.Content = "boredom\n[ˈbɔːdəm]\nзанудство";
            Sound(@"VocaEnglish\Words\boredom.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());

            Sound(@"VocaEnglish\Words\boredom.wav");
            await Task.Delay(2000);
            Stop();

            await Task.Run(() => TimeTextRep());
        }

        private async void TimeTextRep()
        {
            Dispatcher.Invoke(() => lbSubText.Visibility = Visibility.Visible);
            Dispatcher.Invoke(() => lbSubText.Content = "ПОВТОРИТЕ");
            await Task.Run(() => TimerRec());
            Dispatcher.Invoke(() => lbSubText.Visibility = Visibility.Hidden);

            Stop();
            StartFullDuplex1();
            Thread.Sleep(1000);
            Dispatcher.Invoke(() => lbText.Visibility = Visibility.Hidden);
            Thread.Sleep(1000);
            Dispatcher.Invoke(() => lbText.Visibility = Visibility.Visible);
            Thread.Sleep(1000);
            Dispatcher.Invoke(() => lbText.Visibility = Visibility.Hidden);
            Thread.Sleep(1000);
            Dispatcher.Invoke(() => lbText.Visibility = Visibility.Visible);
            Thread.Sleep(1000);
            Stop();
        }

        private void Stop()
        {
            try
            {
                if (mMixer != null)
                {
                    mMixer.Dispose();
                    //mMp3.ToWaveSource(32).Loop().ToSampleSource().Dispose();
                    mMixer = null;
                }
                if (mSoundOut != null)
                {
                    mSoundOut.Stop();
                    mSoundOut.Dispose();
                    mSoundOut = null;
                }
                if (mSoundIn != null)
                {
                    mSoundIn.Stop();
                    mSoundIn.Dispose();
                    mSoundIn = null;
                }
                if (mSource != null)
                {
                    mSource.Dispose();
                    mSource = null;
                }
                if (mMp3 != null)
                {
                    /*if (mDspRec != null)
                    {
                        mDspRec.Dispose();
                    }*/
                    mMp3.Dispose();
                    mMp3 = null;
                }

            }
            catch (Exception ex)
            {
                /*if (langindex == "0")
                {
                    string msg = "Ошибка в Stop: \r\n" + ex.Message;
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
                else
                {
                    string msg = "Error in Stop: \r\n" + ex.Message;
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }*/
            }
        }

        private void Mixer()
        {
            try
            {

                mMixer = new SimpleMixer(1, SampleRate) //стерео, 44,1 КГц
                {
                    FillWithZeros = true,
                    DivideResult = true, //Для этого установлено значение true, чтобы избежать звуков тиков из-за превышения -1 и 1.
                };
            }
            catch (Exception ex)
            {
                if (langindex == "0")
                {
                    string msg = "Ошибка в Mixer: \r\n" + ex.Message;
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
                else
                {
                    string msg = "Error in Mixer: \r\n" + ex.Message;
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
            }
        }

        private void Languages()
        {
            try
            {
                StreamReader FileLanguage = new StreamReader("Data_Language.tmp");
                File.WriteAllText("Data_Load.tmp", "1");
                File.WriteAllText("DataTemp.tmp", "0");
                langindex = FileLanguage.ReadToEnd();

                if (langindex == "0")
                {
                    Title = "ReSelf - Ментальный детокс";
                    lbMicrophone.Content = "Выбор микрофона";
                    lbSpeaker.Content = "Выбор динамиков";
                }
                else
                {
                    Title = "ReSelf - Mental detox";
                    lbMicrophone.Content = "Microphone selection";
                    lbSpeaker.Content = "Speaker selection";
                }

            }
            catch (Exception ex)
            {
                if (langindex == "0")
                {
                    string msg = "Ошибка в Languages: \r\n" + ex.Message;
                    LogClass.LogWrite(msg);
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
                else
                {
                    string msg = "Error in Languages: \r\n" + ex.Message;
                    LogClass.LogWrite(msg);
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
            }
        }

        private void PitchTimerFeelInTheBody()
        {
            int i = 180;
            Dispatcher.Invoke(() => lbTimer.Visibility = Visibility.Visible);
            while (i > 0)
            {
                Dispatcher.Invoke(() => lbTimer.Content = i.ToString());
                pitchVal -= 0.014f;
                SetPitchShiftValue();
                Thread.Sleep(1000);
                i--;
            }
            Dispatcher.Invoke(() => lbTimer.Content = i.ToString());
            Dispatcher.Invoke(() => lbTimer.Visibility = Visibility.Hidden);
        }

        private void btnPlay_MouseMove(object sender, MouseEventArgs e)
        {
            string uri = @"VocaEnglish\Button\button-play-hover.png";
            ImgBackPlay.ImageSource = new ImageSourceConverter().ConvertFromString(uri) as ImageSource;
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            ImgBtnTurboClick = 1;
            EnglishVoca();
            cmbInput.IsEnabled = false;
            cmbOutput.IsEnabled = false;
        }

        private void btnPlay_MouseLeave(object sender, MouseEventArgs e)
        {
            if (ImgBtnTurboClick == 1)
            {
                string uri = @"VocaEnglish\Button\button-play-active.png";
                ImgBackPlay.ImageSource = new ImageSourceConverter().ConvertFromString(uri) as ImageSource;
            }
            else
            {
                string uri = @"VocaEnglish\Button\button-play-inactive.png";
                ImgBackPlay.ImageSource = new ImageSourceConverter().ConvertFromString(uri) as ImageSource;
            }
        }

        //private void TimerPronunciation

        private void VocaEnglish_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SoftCl.IsSoftwareInstalled("Microsoft Visual C++ 2015-2022 Redistributable (x86) - 14.32.31332") == false)
                {
                    Process.Start("VC_redist.x86.exe");
                }

                MMDeviceEnumerator deviceEnum = new MMDeviceEnumerator();
                mInputDevices = deviceEnum.EnumAudioEndpoints(DataFlow.Capture, DeviceState.Active);
                MMDevice activeDevice = deviceEnum.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Multimedia);

                SampleRate = activeDevice.DeviceFormat.SampleRate;

                foreach (MMDevice device in mInputDevices)
                {
                    cmbInput.Items.Add(device.FriendlyName);
                    if (device.DeviceID == activeDevice.DeviceID) cmbInput.SelectedIndex = cmbInput.Items.Count - 1;
                }


                //Находит устройства для вывода звука и заполняет комбобокс
                activeDevice = deviceEnum.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
                mOutputDevices = deviceEnum.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active);

                foreach (MMDevice device in mOutputDevices)
                {
                    cmbOutput.Items.Add(device.FriendlyName);
                    if (device.DeviceID == activeDevice.DeviceID) cmbOutput.SelectedIndex = cmbOutput.Items.Count - 1;
                }

                string[] filename = File.ReadAllLines(fileInfo1.FullName);
                if (filename.Length == 1)
                {
                    Languages();
                }

                if (!File.Exists("log.tmp"))
                {
                    File.Create("log.tmp").Close();
                }
                else
                {
                    if (File.ReadAllLines("log.tmp").Length > 1000)
                    {
                        File.WriteAllText("log.tmp", " ");
                    }
                }
            }
            catch (Exception ex)
            {
                if (langindex == "0")
                {
                    string msg = "Ошибка в Loaded: \r\n" + ex.Message;
                    LogClass.LogWrite(msg);
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
                else
                {
                    string msg = "Error in Loaded: \r\n" + ex.Message;
                    LogClass.LogWrite(msg);
                    MessageBox.Show(msg);
                    Debug.WriteLine(msg);
                }
            }
        }
    }
}
