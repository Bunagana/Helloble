   M S B A M L   `   `   `  ����C  ?Helloble, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null#clr-namespace:HellobleHelloble  5   	       Helloble.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   (localclr-namespace:Helloble     5   	     � Title$  
MainWindow��5   	   $	��450��6   $	��450��6,   $	��850��6<   $	��850��6K   .��5	      �  � RowDefinitions 5
   
   �� 5      $
��211* �6   �� 5      $
��209* �6   .+�5   
   �� -   5        $��	Enumerate��5      $��0q�5   )   $��10,10,657.6,170.2q�65   $	��120��6P   .��6\   	Enumerate6g   �� 5   
   -   5        $��
Disconnect��5      $��0q�5   +   $��161,10,506.6,170.2q�67   $	��120��6S   .��6_   
Disconnect6k   �� 5   
   -   5        $��
ListDevice��5   '   $��
10,46,0,39q�6   $
��Left=�5      $��2��5      $	��250��6$    �� View 5      �� 5      .P�5      �� $��Devices��6%    �� Width$	 250��66    �� DisplayMemberBinding 6B   �� *
LongName+5      5      �� 5   
   -   5        $��ListServices��5   *   $��0,46,277.6,39q�6   $��Right=�5      $	��250��5      $��2��6    5       �� 5!      .P�5"      �� $��Services��6%   $	 250��67    6C   �� *Uuid+5#      5%      �� 5&   
   -   5        $��ListCharacteristics��5&   )   $��0,46,9.6,1.2q�6   $��Right=�5(      $	��250��5)       5*      �� 5+      .P�5,      �� $��Characteristics��6%   $	 250��6>    6J   �� *Uuid+5-      5/      �� 50   
   -   5        $��ListData��50   +   $��534,3.8,9.6,39q�6   $��Right=�52      $��1��53      $	��250��6     54      �� 55      .P�56      �� $��Packet��6%   $	 250��65    6A   �� *
Sequence+57      59      5:      5;      