# NoSurrenderCase

Input -> I used Unity New Input System. Touch for mobile. Enabled Enhanced Touch to use it from windows too.

AI -> Starts overlapSphere with incremental radius. Finds closest edible or player and use it as target. Whenever collision occurs, at the end of the collision sets new destination with same method.

Collision -> When collision occurs only one of them handles it for both players. There is base force + force according to players rotation and scale.

State Manager -> Used an interface called IState to define state actions. There are 2 states:
  RunningState -> User can give inputs to player and player is moving by these inputs.
  SurfingState -> Drifting phase after colliding with another player. During this state user can't give inputs. Player is only drifting according to collide forces.
  
I used State Manager architecture for these 2 states but its slightly big for this kind of small scale game. Reason is that NoSurrender creates casual games and in big scale games, State Manager has a big role. So I did it to show that I can build such systems.


https://youtube.com/shorts/caA4dZAkBJY?feature=share

![Ekran görüntüsü 2023-02-03 171058](https://user-images.githubusercontent.com/76924597/216625858-d5dd43ed-9e63-409a-80d2-073239552e37.png)

![Ekran görüntüsü 2023-02-03 171115](https://user-images.githubusercontent.com/76924597/216625864-1fa90d30-6278-46cf-a43d-97b7d18b62ae.png)

![Ekran görüntüsü 2023-02-03 171124](https://user-images.githubusercontent.com/76924597/216625879-029ae636-39cc-49a2-809c-2f197f625fcd.png)

![Ekran görüntüsü 2023-02-03 171144](https://user-images.githubusercontent.com/76924597/216625885-a0250137-95ce-498e-b76f-09a83a279a57.png)

![Ekran görüntüsü 2023-02-03 171234](https://user-images.githubusercontent.com/76924597/216625893-aad79459-a901-4424-921c-cc934bd98c5b.png)

![Ekran görüntüsü 2023-02-03 171242](https://user-images.githubusercontent.com/76924597/216625897-c6748089-4f86-4d66-823f-0971474eb262.png)
