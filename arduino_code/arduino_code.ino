//int state = 0;  // Tạo biến sự kiện để điều khiển Arduino
//                // Tạo 2 biến để xác định thời gian thực trên Arduino
//long time_now = 0; 
//long time_start = 0;
//float random_number();  // Tạo Random, giả lập dữ liệu cảm biến và đếm thời gian thực
//float data = 0; // Tạo dữ liệu hiển thị của hàm Random
// 
//void setup() {
//  Serial.begin(9600);   // Khởi tạo giao thức Serial, mình chọn baudrate là 9600
//}
// 
//void loop() 
//{
//    // Điều khiển Arduino qua giá trị của biến state
//    if(Serial.available()) 
//    {
//        char temp = Serial.read();
//        if(temp == '0')
//            state = 0;
//        if(temp == '1')
//            state = 1;
//        if(temp == '2')
//            state = 2;
//    }
// 
//    // Thực thi các trường hợp với các giá trị của biến state
//    switch(state)
//    {
//        // state = 0: dừng Arduino
//        case 0:
//        break;
//        // state = 1: thực thi hàm tạo Random, xuất dữ liệu và thời gian thực qua Serial, phân tách nhau bởi ký tự gạch đứng “|”
//        case 1:
//            random_number();
//            Serial.print(time_now);
//            Serial.print("|");
//            Serial.println(data);
//        break;
//        // state = 2: Reset dữ liệu và thời gian về 0
//        case 2:
//            data = 0;
//            time_now = 0;
//            state = 0;
//        break;
//    }
//}
// 
//// Hàm tạo Random và đếm thời gian, mình muốn tạo số thực từ 0,001 đến 1000
//float random_number()
//{
//    time_start = millis();
//    data = random(1,1000000); 
//    data = data/1000;
//    delay(500);
//    time_now = time_now + millis() - time_start;
//}
