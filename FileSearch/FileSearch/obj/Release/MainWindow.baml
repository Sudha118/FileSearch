   M S B A M L   `   `   `  ����   
FileSearch    FileSearch.MainWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     -     � Title$  File Search��$��396.666��$��599.243�� � Icon$ 
search.ico��.��� .+��� .@��� $��Search��.��� ���	#FFE5E5E5$��0,0,0,-1q� � ColumnDefinitions �� $	��72* ��� $	��14* ��� $
��499* �.+��� -    ��Name$ txtPath��$��23��$��
10,30,47,0q� �� TextWrapping$
 Wrap=�$	��Top=�$��2���� -    ��Name$ 	btnBrowse��$	��...��$��0,30,5,0q�$	��Top=�$��Right=�$��37��$��2���� -   $ txtSearchString��$��23��$��
10,58,98,0q�$
 Wrap=�$	��Top=�$��2���� $��Search Text��$
��Left=�$��5,53,0,0q�$	��Top=�$��2���� -   $ btnStart��$��Start��$��0,59,5,0q�$	��Top=�$��Right=�$��75��$��2���� $��Results��$
��Left=�$��	5,102,0,0q�$	��Top=��� -    ��Name$ 	lblStatus��$�� ��$��5,0,5,1q�$��26��$��Bottom=�$��3��� -    �Name$ pbProgressFiles��$��16��$��
32,109,5,0q�$	��Top=�$��2���� $��Files��$
��Left=�$��12.896,103,0,0q�$	��Top=�$��24��$��2��$��1��&   System.Windows.Controls.DataGrid  -     Name$ gridDataView��$��
5,134,5,27q�	   SelectionUnit$	 CellOrRowHeader=�$��3������ 
   AlternatingRowBackground
 ��  �� EndPoint$ 0.5,1&� �� 
StartPoint$ 0.5,0&�.���  � Color$ 	#FFF7FAFB�� � Offset$ 0Q�� $ 	#FFAFE2F1��$ 1Q��� $
��Path��$
��Left=�$��	23,32,0,0q�$	��Top=�