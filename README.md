# Client

## 실행 환경
- .NET 4.5.2 이상
- Releas.zip

---
## Client : 채팅 클라이언트

### 실행
__App.config__ 파일에서 <appSettings>에 `key =  "ip,port"`를 추가한다. (ip와 port를 ,로 구별, 띄어쓰기 없음)
``` html 
  <appSettings>
    <add key="10.100.58.9,11000"/>
    <add key="10.100.58.9,12000"/>
    <add key="10.100.58.9,13000"/>
    <add key="10.100.58.9,14000"/>
  </appSettings>
```

### 기능
- Chatting
- Health Check
- Connect Passing

---

## Dummy : 더미 클라이언트
- 정해진 프로토콜에 따라 채팅 룸까지 들어와서 무작위 개수의 현재 시간 정보를 출력하고 5~10초 사이 기다린 후 로그아웃

### 실행
1 __App.config__ 파일에서 <appSettings>에 `key =  "ip,port"`를 추가한다. (ip와 port를 ,로 구별, 띄어쓰기 없음)
``` html 
  <appSettings>
    <add key="10.100.58.9,11000"/>
    <add key="10.100.58.9,12000"/>
    <add key="10.100.58.9,13000"/>
    <add key="10.100.58.9,14000"/>
  </appSettings>
```
2 `Run Project` > `Run.cs` > `Main.cs` >  `Process.Start()`  함수안에 `Dummy.exe` 파일 위치를 넣는다.
``` C#
 Process.Start("C:\\Users\\Yungyung\\Documents\\Visual Studio 2015\\Projects\\Chatting\\Dummy\\bin\\Release\\Dummy.exe","dummy"+i);
```
3 `Run Project`를 통해 Dummy Client 시작 가능

### 로직
 <pre>            
      (성공)                                                        (RoomList에서 랜덤 Room No를 뽑아 입장)
Login ----------------------------------> Loby ----> Room List 조회 ---------------------------------> Room ------> ...
      \                                /                            \                               / 
       ----> Signup -----> Login ---->                               ---------> Create Room ------->
      (실패)                                                         (RoomList가 비어잇으면)      
      
      
... ------> 무작위 개수의 현재 시간 정보를 출력 -----> 5~10초 사이 기다린 후 -----> LogOut -------> Exit
  </pre>     
  
---

## Monitoring : 현재 채팅 시스템 상황 보여주는 모니터링 클라이언트
- 채팅 룸 개수, FE 서버별 사용자 수(더미 클라이언트 포함), 채팅메시지 수 작성 TOP10을 실시간으로 보여줌(더미 제외)

### 실행

### 기능

