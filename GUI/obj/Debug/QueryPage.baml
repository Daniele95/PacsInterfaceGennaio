   M S B A M L   `   `   `  ����  GUI    GUI.QueryPage   5      W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      >d2http://schemas.microsoft.com/expression/blend/2008    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5        N� Title$  	QueryPage��5      .��5
      � .+�5   
   �� -   5         ��Name$ Search��5      $��Search��6;   $
��Left=�6L   $��
414,78,0,0q�6g   $	��Top=�6{   $��75��6�   �� 5   
   -   5         ��Name$ PatientNameBox��5      $��Right=�6D   $��23��6`   $��
0,40,585,0q�6l    �� TextWrapping$
 Wrap=�6�   $�� ��6�   $	��Top=�6�   $	��120��6�   �� 5   
   -   5         ��Name$ PatientNameBlock��5      $
��Left=�6.   $��	20,44,0,0q�6I   $
��Wrap=�6\   $
��Name��6p   $	��Top=�6|   $��23��6�   $��33��6�   �� 5   
   -   5        $ PatientSurNameBlock��5      $
��Left=�61   $��	20,15,0,0q�6L   $
��Wrap=�6_   $��Surname��6s   $	��Top=�6�   $��23��6�   $��48��6�   $��1��6�   �� 5   
   -   5        $ ModalityBox��5      $
��Left=�6A   $��23��6\   $��
609,44,0,0q�6h   $
 Wrap=�6|   $�� ��6�   $	��Top=�6�   $	��120��6�    J� RenderTransformOrigin$ 0.242,0.304&�6�   �� 5   
   -   5        $ StudyDateBlock��5      $
��Left=�6,   $��
215,18,0,0q�6G   $
��Wrap=�6[   $��Study Date Min��6o   $	��Top=�6�   $��22��6�   $	��120��6�   �� 5   
   -   5        $ StudyDateBlockEnd��5      $
��Left=�6/   $��
215,46,0,0q�6J   $
��Wrap=�6^   $��Study Date Max��6r   $	��Top=�6�   $��22��6�   $	��120��6�   �� 5   
   -   5        $ ModalityBlock��5      $��480,45,215,0q�6+   $
��Wrap=�6A   $��Modalities in study��6U   $	��Top=�6p   $��33��6�   (  "System.Windows.Controls.DatePicker  5   
   -	   5        $��StudyDateStartPicker��5      $
��Left=�6K   $��
335,16,0,0q�6f   $	��Top=�6z     5   
   -
   5          Name$ StudyDateEndPicker��5      $
��Left=�6K   $��
335,45,0,0q�6f   $	��Top=�6z   �� 5   
   -   5        $ PatientSurnameBox��5      $
��Left=�6G   $��23��6b   $��	73,15,0,0q�6n   $
 Wrap=�6�   $�� ��6�   $	��Top=�6�   $	��120��6�   �� 5   
   -   5         ��Name$ listView��5      $
��Left=�6>   $	��243��6Y   $��
10,109,0,0q�6f   $	��Top=�6z   $	��662��6�   ��5      �� 5       �� 
TargetType" ��6   .�5      -   5        5      	 �� View	 5       �� 5!      -   5        gridView��5"      5$      �� 5%   
   -   5        $ localSearch��5%      $��Search local��6A   $
��Left=�6X   $��
231,78,0,0q�6s   $	��Top=�6�   $��75��6�   �� 5&   
   -   5        $ PatientIDBox��5&      $
��Left=�6B   $��23��6]   $��
609,18,0,0q�6i   $
 Wrap=�6}   $�� ��6�   $	��Top=�6�   $	��120��6�   $ 0.242,0.304&�6�   �� 5'   
   -   5        $ PatientIDBlock��5'      $��480,16,234,0q�6,   $
��Wrap=�6B   $��
Patient ID��6V   $	��Top=�6h   $��25��6�   5)      5*      