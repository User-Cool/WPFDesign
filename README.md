# WPFDesign
WPF界面设计框架思想学习。

## 前言
WPF框架很好的实现了界面和逻辑的分离，可以使界面设计和逻辑设计同步进行。在先进思想的支撑下，WPF框架界面设计灵活，其实现的效果和HTML不相上下。
为详细研究WPF的各种思想和技巧创建该测试项目，目的在于对WPF所包含的思想、技巧做深入的了解，以便能在其他地方使用。

## MVVM
MVVM即Model、View、ViewModel，MVVM思想实现了界面和业务的完全分离。
+ Model（模型）是从业务的角度设计的，是业务的抽象，因为项目可能是全能的，只可能面向部分某个方面，因此该部分是项目中相对稳定的部分；
+ View（界面）是与用户进行交互的接口，从用户体验的角度进行设计，由于用户的不确定，这部分是项目中最容易发生变化的部分；
+ ViewModel（界面模型）是Model和View的桥梁，Model和View在设计的时候是分开的，并没有考虑彼此，因此无法直接连接起来，这就需要一个中间模块来实现Model和View的通信。ViewModel将Model数据转换成View需要的类型，将View数据转换成Model需要的类型。

## 数据、算法（控制）
WPF对程序项目进行了抽象，认为程序是由**数据**和控制数据的**算法**组成的。这种抽象为我们提供了一个不一样的角度去看程序和模块。

## 容器思想（Container），控件是容器+内容
杯子可以盛水，电脑放在电脑桌上，还有箱子、盒子等存杂物的容器。生活中有各种各样的容器来存放各种各样的物品。
我自认为看不出这和编程有什么关系，但是WPF的设计者显然不这样认为，他们成功的将容器和控件联系在了一起。
将控件抽象成容器是一种很好的抽象。以Button为例，我们希望Button可以显示文字，希望可以显示图片，也希望可以显示视频或者其他什么。按钮的设计者肯定想不到
按钮的使用者到底想要在按钮上显示什么，并且哪怕知道了，也不太可能将所有可能显示的项封装到按钮中，者工作量太大了。容器设计思想的使用彻底解决了这个问题，
按钮被设计成容器，不再在内部对显示数据进行定义，使用者想要显示什么，就像里面添加什么，在xmal设计时添加上。

## 渲染树、逻辑树
这个来自浏览器？WPF的渲染采样树形结构，并且有多个不同的树，Visual树是最终的渲染树。
注意：不要从渲染树中找数据，渲染树是实时的，渲染树只包含“可见”的内容。

## Template模板思想
模板思想的使用让WPF界面设计变的非常灵活。思想依据是数据、算法的程序抽象思想，一切都是有算法和数据组成的。站在控件的角度上，控件就是控制算法（容器），承载数据（内容）。
+ ControlTemplate容器模板，描述的是控件。
+ DataTemplate数据模板，描述的控件的数据。
Template设计思想是的控件设计更加灵活，不仅数据可以使用xaml定义，就连数据在容器中怎么放都可以使用xaml定义。
容器和内容都可以自定义，势必会破坏原本的控件结构，也就是在修改ControlTemplate控制容器样式的时候，我们必须告诉ControlTemplate DataTemplate放在那里。
为了解决这个问题，WPF设计了一些Presenter，来代指DataTemplate，在ControlTemplate中占位。


