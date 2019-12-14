// adc converter
#define ADC_ITER_CNT      5
#define ADC_DB_RES        60.0 / 1024.0
#define ADC_DB_CENTER     1024 / 2
#define ADC_DB_OFFSET     (-30.0)
#define ADC_DEG_RES       180.0 / 1024.0
// analog read pins
#define PIN_SWR_AMP       A0
#define PIN_SWR_PHS       A1
// utils
#define DEG_TO_RAD(deg)   (deg * 3.14159 / 180.0)
#define TO_MHZ(freq)      (freq / 1000000)


// one measurement point
struct measurement_t 
{  
  uint32_t freq_khz;

  // read from adc
  int16_t amp;      // amplitude
  int16_t phs;      // phase

  // adjusted by calibration
  int16_t amp_adj;
  int16_t phs_adj;
  
  float rl_db;    // return loss, S11
  float phi_deg;  // phase shift angle
  
  float rho;


  // dùng để vẽ smith chart
  float rs;       // real impedance part
  float xs;       // complex impedance part
  
  float swr;
  float z;        // impedance vector length
};

/* --------------------------------------------------------------------------*/
struct measurement_t g_pt;




/* --------------------------------------------------------------------------*/

void swr_measure()// đo 5 lần lấy trung bình
{
  
  g_pt.amp = 0;
  g_pt.phs = 0;

  for (uint8_t i = 0; i < ADC_ITER_CNT; i++) 
  {
    g_pt.amp += analogRead(PIN_SWR_AMP);
    g_pt.phs += analogRead(PIN_SWR_PHS);
  }

  g_pt.amp /= ADC_ITER_CNT;
  g_pt.phs /= ADC_ITER_CNT;
}

void swr_calculate()
{
  // hiệu chỉnh số liệu theo calib (kế hoạch)
  // g_pt.amp_adj = swr_amp_cal_adjust(g_pt.amp);
  // g_pt.phs_adj = swr_phs_cal_adjust(g_pt.phs);

  g_pt.rl_db = fabs(((float)g_pt.amp * ADC_DB_RES) + ADC_DB_OFFSET);
  g_pt.phi_deg = ((float)g_pt.phs * ADC_DEG_RES);

  g_pt.rho = pow(10.0, g_pt.rl_db / -20.0);   //  phức hóa S11
  
  float re = g_pt.rho * cos(DEG_TO_RAD(g_pt.phi_deg));
  float im = g_pt.rho * sin(DEG_TO_RAD(g_pt.phi_deg));
  
  float denominator = ((1 - re) * (1 - re) + (im * im));  // mẫu số 
  
  g_pt.rs = fabs((1 - (re * re) - (im * im)) / denominator) * 50.0;
  g_pt.xs = fabs(2.0 * im) / denominator * 50.0;
  
  g_pt.z = sqrt(g_pt.rs * g_pt.rs + g_pt.xs * g_pt.xs);
  
  g_pt.swr = fabs(1.0 + g_pt.rho) / (1.001 - g_pt.rho);

  g_pt.rl_db *= -1;
}

void reflectometer_initialize()
{  
  // from AD8302 gain/phase detector
  analogReference(EXTERNAL) ;
  analogRead(PIN_SWR_AMP);
  analogRead(PIN_SWR_PHS);
}







/* --------------------------------------------------------------------------*/
void setup() {
  Serial.begin(9600);  
  reflectometer_initialize();

}






  
void loop(){
  
  
  
  
  }
