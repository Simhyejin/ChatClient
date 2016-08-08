- [Protocol](https://github.com/lunker/ChatProtocol)
- [Client](#client)
- [FrontEndServer](https://github.com/kimsin3003/ChatServer/tree/Release)
- [BackendServer](https://github.com/lunker/lunkerRedis)

---

#<div id ="client"> Client </div>

## 실행 환경
- .NET 4.5.2 이상

## Client : 채팅 클라이언트

### 실행
<pre>1. `Client.exe.config`파일에서 __App.config__ 의 <appSettings>에 `key =  "ip,port"`를 추가한다. </pre>
(ip와 port를 ,로 구별, 띄어쓰기 없음)

``` html 
  <appSettings>
    <add key="10.100.58.9,11000"/>
    <add key="10.100.58.9,12000"/>
    <add key="10.100.58.9,13000"/>
    <add key="10.100.58.9,14000"/>
  </appSettings>
```
<pre>2. client.exe 를 실행</pre>

### 기능
- Chatting
- Health Check
- Connect Passing

---

## Dummy : 더미 클라이언트

### 실행
<pre>1. __App.config__ 파일에서 <appSettings>에 `key =  "ip,port"`를 추가한다.</pre>
(ip와 port를 ,로 구별, 띄어쓰기 없음)

``` html 
  <appSettings>
    <add key="10.100.58.9,11000"/>
    <add key="10.100.58.9,12000"/>
    <add key="10.100.58.9,13000"/>
    <add key="10.100.58.9,14000"/>
  </appSettings>
```
<pre>2. Release 파일이 위피한 폴더에서 콘솔창을 열어 `dummy id`를 입력한다. </pre>
(단, id는 dummy로 시작하며 뒤에 숫자가 붙을 수 있다. 예시, dummy1, dummy2, ...)

### 기능
- 정해진 프로토콜에 따라 채팅 룸 입장
- 무작위 개수의 현재 시간 정보를 출력
- 5~10초 사이 기다린 후 로그아웃

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
### 실행
1. Release 파일이 위피한 폴더에서 콘솔창을 열어 `admin ip port`를 입력한다.
3. Admin.exe 를 실행
2. 아이디 : admin 비밀번호 : root

### 기능
- 채팅 룸 개수
-  FE 서버별 사용자 수(더미 클라이언트 포함)
-  채팅메시지 수 작성 TOP10을 실시간으로 보여줌(더미 제외) (6초마다 갱신)

