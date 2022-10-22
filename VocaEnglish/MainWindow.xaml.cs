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
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace VocaEnglish
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessageW(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("winmm.dll")]
        public static extern int waveOutGetVolume(IntPtr hwo, out uint pdwVolume);

        [DllImport("winmm.dll")]
        public static extern int waveOutSetVolume(IntPtr hwo, uint dwVolume);

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
        private int SampleRate, SoundClick = 0, ImgBtnTurboClick = 0, countRepeat = 0, countMassive = 0;

        string langindex;

        private const int APPCOMMAND_VOLUME_UP = 0xA0000;
        private const int APPCOMMAND_VOLUME_DOWN = 0x90000;
        private const int WM_APPCOMMAND = 0x319;

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
                //pitchVal = -1.5f;

                //SetPitchShiftValue();

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
            int i = 10;
            while (i > 0)
            {
                Dispatcher.Invoke(() => lbTimer.Content = i.ToString());
                Dispatcher.Invoke(() => lbTranscription.Visibility = Visibility.Hidden);
                Dispatcher.Invoke(() => lbRussianWords.Visibility = Visibility.Hidden);
                pitchVal = -1.0f;
                SetPitchShiftValue();
                Thread.Sleep(500);
                Dispatcher.Invoke(() => lbTranscription.Visibility = Visibility.Visible);
                Dispatcher.Invoke(() => lbRussianWords.Visibility = Visibility.Visible);
                pitchVal = 1.0f;
                SetPitchShiftValue();
                Thread.Sleep(500);
                i--;
            }
            Dispatcher.Invoke(() => lbTimer.Content = i.ToString());
            Dispatcher.Invoke(() => lbTimer.Visibility = Visibility.Hidden);
        }

        private async void EnglishVoca()
        {
            Feeling_in_the_body_pattern();

            btnPlay.Visibility = Visibility.Hidden;

            lbSubText.Content = "Сейчас начнется трёх минутная подготовка\n                         голоса и слуха\n                     держите звук 'ААА'";
            lbSubText.Visibility = Visibility.Visible;
            await Task.Delay(6000);
            lbSubText.Visibility = Visibility.Hidden; 

            Stop();
            StartFullDuplex();
            await Task.Run(() => PitchTimerFeelInTheBody());
            Stop();

            lbSubText.Content = "      Хорошо. Сейчас начнут появляться слова на экране\nслушайте их, и повторняйте их, после фразы 'ПОВТОРИТЕ'";
            lbSubText.Visibility = Visibility.Visible;
            await Task.Delay(6000);

            await Task.Run(() => EnglishVocaSuper());
           
        }

        private async void EnglishVocaSuper()
        {
            Dispatcher.Invoke(() => lbRussianWords.Visibility = Visibility.Visible);
            Dispatcher.Invoke(() => lbTranscription.Visibility = Visibility.Visible);
            Dispatcher.Invoke(() => lbText.Visibility = Visibility.Visible);
            while (countMassive < ListWords.Values)
            {
                while (countRepeat < 2)
                {
                    Dispatcher.Invoke(() => lbText.Content = ListWords.EnWords[countMassive]);
                    Dispatcher.Invoke(() => lbTranscription.Content = ListWords.Transcription[countMassive]);
                    Dispatcher.Invoke(() => lbRussianWords.Content = ListWords.RuWords[countMassive]);
                    Sound(@"VocaEnglish\Words\" + ListWords.EnWords[countMassive] + ".wav");
                    await Task.Delay(2000);
                    Stop();

                    Dispatcher.Invoke(() => lbSubText.Visibility = Visibility.Visible);
                    Dispatcher.Invoke(() => lbSubText.Content = "ПОВТОРИТЕ");
                    StartFullDuplex1();
                    await Task.Run(() => TimerRec());
                    Dispatcher.Invoke(() => lbSubText.Visibility = Visibility.Hidden);

                    Stop();

                    countRepeat++;
                }
                countRepeat = 0;
                countMassive++;
            }
            await Task.Delay(2000);
            Application.Current.Dispatcher.Invoke(() => 
            { 
                Remember remember = new Remember();
                remember.ShowDialog();
                Close();
            });
            
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
                    //Title = "ReSelf - Ментальный детокс";
                    lbMicrophone.Content = "Выбор микрофона";
                    lbSpeaker.Content = "Выбор динамиков";
                }
                else
                {
                    //Title = "ReSelf - Mental detox";
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

        private void VocaEnglish_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Stop();
            Environment.Exit(0);
        }

        private void btnPlay_MouseMove(object sender, MouseEventArgs e)
        {
            string uri = @"VocaEnglish\Button\button-play-hover.png";
            ImgBackPlay.ImageSource = new ImageSourceConverter().ConvertFromString(uri) as ImageSource;
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            /*Remember remember = new Remember();
            remember.ShowDialog();*/
            ImgBtnTurboClick = 1;
            EnglishVoca();
            cmbInput.Visibility = Visibility.Hidden;
            lbMicrophone.Visibility = Visibility.Hidden;
            cmbOutput.Visibility = Visibility.Hidden;
            lbSpeaker.Visibility = Visibility.Hidden;
            
            
        }

        private void btnIncVol_Click(object sender, RoutedEventArgs e)
        {
            var wih = new WindowInteropHelper(this);
            var hWnd = wih.Handle;
            SendMessageW(hWnd, WM_APPCOMMAND, hWnd, (IntPtr)APPCOMMAND_VOLUME_UP);

            string uri = @"VocaEnglish\Button\button-soundup-active.png";
            ImgBtnIncVol.ImageSource = new ImageSourceConverter().ConvertFromString(uri) as ImageSource;
        }

        private void btnDecVol_Click(object sender, RoutedEventArgs e)
        {
            var wih = new WindowInteropHelper(this);
            var hWnd = wih.Handle;
            SendMessageW(hWnd, WM_APPCOMMAND, hWnd, (IntPtr)APPCOMMAND_VOLUME_DOWN);

            string uri = @"VocaEnglish\Button\button-sounddown-active.png";
            ImgBtnDecVol.ImageSource = new ImageSourceConverter().ConvertFromString(uri) as ImageSource;
        }

        private void btnIncVol_MouseMove(object sender, MouseEventArgs e)
        {
            string uri = @"VocaEnglish\Button\button-soundup-hover.png";
            ImgBtnIncVol.ImageSource = new ImageSourceConverter().ConvertFromString(uri) as ImageSource;
        }

        private void btnIncVol_MouseLeave(object sender, MouseEventArgs e)
        {
            string uri = @"VocaEnglish\Button\button-soundup-inactive.png";
            ImgBtnIncVol.ImageSource = new ImageSourceConverter().ConvertFromString(uri) as ImageSource;
        }

        private void btnDecVol_MouseMove(object sender, MouseEventArgs e)
        {
            string uri = @"VocaEnglish\Button\button-sounddown-hover.png";
            ImgBtnDecVol.ImageSource = new ImageSourceConverter().ConvertFromString(uri) as ImageSource;
        }

        private void btnDecVol_MouseLeave(object sender, MouseEventArgs e)
        {
            string uri = @"VocaEnglish\Button\button-sounddown-inactive.png";
            ImgBtnDecVol.ImageSource = new ImageSourceConverter().ConvertFromString(uri) as ImageSource;
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

        private void VolDec()
        {
            int i = 50;
            while(i > 0) {
                var wih = new WindowInteropHelper(this);
                var hWnd = wih.Handle;
                SendMessageW(hWnd, WM_APPCOMMAND, hWnd, (IntPtr)APPCOMMAND_VOLUME_DOWN);
                i--;
            }
        }

        private void VolInc()
        {
            int i = 0;
            while (i < 10)
            {
                var wih = new WindowInteropHelper(this);
                var hWnd = wih.Handle;
                SendMessageW(hWnd, WM_APPCOMMAND, hWnd, (IntPtr)APPCOMMAND_VOLUME_UP);
                i++;
            }
        }

        //private void TimerPronunciation

        private void VocaEnglish_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxSpeak boxSpeak = new MessageBoxSpeak();
                boxSpeak.ShowDialog();

                

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

                VolDec();

                VolInc();
                ListWords list = new ListWords();
                list.List();

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
