<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="datadisplay.aspx.cs" Inherits="CCCloud.datadisplay" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title>物联网工程系2019级姚锋毕业设计</title>
    <link href="CSS/bootstrap.min.css" rel="stylesheet" />
    <link href="CSS/app.css" rel="stylesheet" />
    <!-- 视频模块 css-->
    <link href="CSS/video-js.css" rel="stylesheet" />
    <script src="JavaScript/videojs-flash.min.js"></script>
</head>
<body class="bg06">
    <form id="form1" runat="server">
    <header class="header">
        <h3>远程驾驶监控系统
        </h3>
       
    </header>
    
     <div class="wrapper">
         <div class="container-fluid">
             <div class="row fill-h">
                 <div class="col-lg-3 fill-h">
                     <div class="xpanel-wrapper xpanel-wrapper-1-2">
                         <div class="xpanel">
                            <!-- 速度表盘 -->
                            <div class="fill-h" id="instrumentPanel"></div>
                         </div>
                     </div>
                     <div class="xpanel-wrapper xpanel-wrapper-1-2">
                         <div class="xpanel">
                             <!--建议速度-->
                             <div class="fill-h" id="ballChart"></div>
                         </div>
                     </div>
                 </div>
                 <div class="col-lg-6 fill-h">
                     <div class="xpanel-wrapper xpanel-wrapper-1">
                         <div class="xpanel">
                             <!-- 视频 video-->
                            <div class="fill-h" id="video">
                                <script src="JavaScript/video.js"></script>
                                <video id="carlive"  class="video-js vjs-default-skin vjs-big-play-centered" style="height:100%;width:100%"  autoplay="autoplay" poster="//vjs.zencdn.net/v/oceans.png" data-setup='{}'>
     		                    <!--src: 规定媒体文件的 URL  type:规定媒体资源的类型-->
    		                    <source src='rtmp://120.78.162.85/carlive/test' type='rtmp/flv'/>
		                        </video>
		                        <script>
		                            videojs.options.flash.swf = "VideoJS.swf";
		                            var myplay = videojs("carlive"); //播放
		                            myplay.player(); // 暂停
		                        </script>
                            </div>
                         </div>
                     </div>
                 </div>
                 <div class="col-lg-3 fill-h">
                     <div class="xpanel-wrapper xpanel-wrapper-2-3">
                         <div class="xpanel">
                             <!-- 地图relationChart -->
                             <div class="fill-h" id="relationChart"></div>
                         </div>
                     </div>
                     <div class="xpanel-wrapper xpanel-wrapper-1-3">
                         <div class="xpanel">
                             <!-- traffic_light -->
                             <div class="fill-h" id="mapChart">
                                 <div class="traffic_light">15</div>

                             </div>
                         </div>
                     </div>
                 </div>
             </div>
         </div>
     </div>

    <script src="JavaScript/jquery-3.3.1.min.js"></script>
    <script src="JavaScript/echarts-3.8.5.min.js"></script>
    <script src="JavaScript/echarts-map-china.js"></script>
    
    <script src="http://echarts.baidu.com/build/dist/echarts.js"></script>
    <script type="text/javascript" src="http://api.map.baidu.com/api?v=2.0&ak=sFr96SmyHqPNbORWwYhMcZ5Eblj2LWVn"></script>

    <script type="text/javascript">
        window.onbeforeunload = function () {
            if (event.clientX > document.body.clientWidth && event.clientY < 0 || event.altKey)
                ToggleConnectionClicked();
            else
                alert("刚刚刷新页面！");
        }
        // 路径配置
        require.config({
            paths: {
                echarts: 'http://echarts.baidu.com/build/dist'
            }
        });

        // 使用
        require(
            [
                'echarts',
                'echarts/chart/bar' // 使用柱状图就加载bar模块，按需加载
            ],
            
        function () {
            
           

            /*************** 柱状图 **************/

            //初始化echarts实例
            var myChart = echarts.init(document.getElementById('ballChart'));
            var option = {
                tooltip: {
                    formatter: "{a} <br/>{b} : {c}%"
                },
                toolbox: {
                    show: true,
                    feature: {
                        mark: { show: true },
                        restore: { show: true },
                        saveAsImage: { show: true }
                    }
                },
                series: [
                    {
                        name: '速度指标',
                        type: 'gauge',
                        startAngle: 180,
                        endAngle: 0,
                        center: ['50%', '90%'],    // 默认全局居中
                        radius: 210,
                        axisLine: {            // 坐标轴线
                            lineStyle: {       // 属性lineStyle控制线条样式
                                width: 100
                            }
                        },
                        axisTick: {            // 坐标轴小标记
                            splitNumber: 10,   // 每份split细分多少段
                            length: 12,        // 属性length控制线长
                        },
                        axisLabel: {           // 坐标轴文本标签，详见axis.axisLabel
                            formatter: function (v) {
                                switch (v + '') {
                                    case '10': return '低';
                                    case '50': return '中';
                                    case '90': return '高';
                                    default: return '';
                                }
                            },
                            textStyle: {       // 其余属性默认使用全局文本样式，详见TEXTSTYLE
                                color: '#fff',
                                fontSize: 15,
                                fontWeight: 'bolder'
                            }
                        },
                        pointer: {
                            width: 20,
                            length: '80%',
                            color: 'rgba(255, 255, 255, 0.8)'
                        },
                        title: {
                            show: true,
                            offsetCenter: [0, '-60%'],       // x, y，单位px
                            textStyle: {       // 其余属性默认使用全局文本样式，详见TEXTSTYLE
                                color: '#fff',
                                fontSize: 30
                            }
                        },
                        detail: {
                            show: true,
                            backgroundColor: 'rgba(0,0,0,0)',
                            borderWidth: 0,
                            borderColor: '#ccc',
                            width: 100,
                            height: 40,
                            offsetCenter: [0, -40],       // x, y，单位px
                            formatter: '{value}km/h',
                            textStyle: {       // 其余属性默认使用全局文本样式，详见TEXTSTYLE
                                fontSize: 50
                            }
                        },
                        data: [{ value: 25, name: '建议速度' }]
                    }
                ]
            };
           
            myChart.setOption(option);

            /*************** 表盘 **************/
            //初始化echarts实例
            var instrument_panel = echarts.init(document.getElementById('instrumentPanel'));
            var instrumentPanel = {
                tooltip: { formatter: "{a} <br/>{b} : {c}迈" },
                series: [
                {
                    name: '速度指标',
                    type: 'gauge',
                    detail: { formatter: '{value}km/H' }, //仪表盘显示数据
                    axisLine: { //仪表盘轴线样式
                        lineStyle: { width: 20 }
                    },
                    splitLine: { //分割线样式
                        length: 20
                    },
                    data: [{ value: 0, name: '速度' }]
                }
                ]
            };
            
            /*********汽车GPS可视化**********/
            //数据准备
            //数据准备,
            var points = [];//原始点信息数组
            var bPoints = [];//百度化坐标数组。用于更新显示范围。

            //地图操作开始
            var map = new BMap.Map("relationChart");
            map.centerAndZoom(new BMap.Point(120.781443, 31.592896), 19); //初始显示中国。
            map.enableScrollWheelZoom();//滚轮放大缩小
            //添加线
            function addLine(points) {
                var linePoints = [], pointsLen = points.length, i, polyline;
                if (pointsLen == 0) {
                    return;
                }
                // 创建标注对象并添加到地图   
                for (i = 0; i < pointsLen; i++) {
                    linePoints.push(new BMap.Point(points[i].lng, points[i].lat));
                }
                polyline = new BMap.Polyline(linePoints, { strokeColor: "red", strokeWeight: 2, strokeOpacity: 0.5 });   //创建折线
                map.addOverlay(polyline);   //增加折线
            }
            //根据点信息实时更新地图显示范围，让轨迹完整显示。设置新的中心点和显示级别
            function setZoom(bPoints) {
                var view = map.getViewport(eval(bPoints));
                var mapZoom = view.zoom;
                var centerPoint = view.center;
                map.centerAndZoom(centerPoint, mapZoom);
            }
            //在轨迹点上创建图标，并添加点击事件，显示轨迹点信息。points,数组。
            function addMarker(points) {
                var pointsLen = points.length;
                if (pointsLen == 0) {
                    return;
                }
                var myIcon = new BMap.Icon("./Img/track.ico", new BMap.Size(5, 5), {
                    offset: new BMap.Size(5, 5)
                });
                // 创建标注对象并添加到地图   
                for (var i = 0; i < pointsLen; i++) {
                    var point = new BMap.Point(points[i].lng, points[i].lat);
                    var marker = new BMap.Marker(point, { icon: myIcon });
                    map.addOverlay(marker);
                }
            }

            /********** 窗口大小改变时，重置报表大小 ********************/
            window.onresize = function () {
                instrumentPanel.resize();
                ballChart.resize();
                relationChart.resize();
                video.resize();
                mapChart.resize();
            };

            var ws;
            var SocketCreated = false;
            var isUserloggedout = false;


            function ToggleConnectionClicked() {
                if (SocketCreated && (ws.readyState == 0 || ws.readyState == 1)) {
                    SocketCreated = false;
                    isUserloggedout = true;
                    ws.close();
                } else {
                    try {
                        if ("WebSocket" in window) {
                            ws = new WebSocket("ws://" + "120.78.162.85:4399/start");
                        }
                        else if ("MozWebSocket" in window) {
                            ws = new MozWebSocket("ws://" + "120.78.162.85:4399/start");
                        }
                        SocketCreated = true;
                        isUserloggedout = false;
                    } catch (ex) {
                        return;
                    }
                    ws.onopen = WSonOpen;
                    ws.onmessage = WSonMessage;
                    ws.onclose = WSonClose;
                    ws.onerror = WSonError;
                }
            };

            function WSonOpen() {
                ws.send("login:" + "姚锋");
            };
            var id=1;
            function WSonMessage(event) {
                var string = event.data;
                var arr = string.split(",");
               
                // alert(arr[0]);
                //速度盘可视化
                instrumentPanel.series[0].data[0].value = arr[0];
                option.series[0].data[0].value = (Math.random() * 14).toFixed(2) - 0;
                instrument_panel.setOption(instrumentPanel, true);

                //GPS数据处理
                var lng = arr[6];
                var lat = arr[7];
                id = id + 1;
                var point = { "lng": lng, "lat": lat, "status": 1, "id": id }
                var makerPoints = [];
                var newLinePoints = [];
                var len;

                makerPoints.push(point);
                addMarker(makerPoints);//增加对应该的轨迹点
                points.push(point);
                bPoints.push(new BMap.Point(lng, lat));
                len = points.length;
                newLinePoints = points.slice(len - 2, len);//最后两个点用来画线。

                addLine(newLinePoints);//增加轨迹线
                setZoom(bPoints);

                var traffic_light = document.getElementsByClassName('traffic_light')[0];
               /*
                    switch (num) {
                        case -1:
                            traffic_light.style.backgroundColor = 'gray'; break;
                        case 0:
                            traffic_light.style.backgroundColor = 'green'; break;
                        case 1:
                            traffic_light.style.backgroundColor = 'yellow'; break;
                        case 2:
                        case 3:
                            traffic_light.style.backgroundColor = 'red'; break;
                    }
                   // traffic_light.innerText = time;*/
                if (arr[5] == -1)
                {
                    traffic_light.style.backgroundColor = 'gray';
                }
                else if (arr[5] == 0)
                {
                    traffic_light.style.backgroundColor = 'green';
                }
                else if (arr[5] == 1)
                {
                    traffic_light.style.backgroundColor = 'yellow';
                }
                else
                {
                    traffic_light.style.backgroundColor = 'red';
                }
                // traffic_light.innerText = arr[8];
            };

            function WSonClose() {

            };

            function WSonError() {

            };

            function SendDataClicked() {

            };


            $(document).ready(function () {
                var WebSocketsExist = true;
                try {
                    var dummy = new WebSocket("ws://120.78.162.85:4399/start");
                } catch (ex) {
                    try {
                        webSocket = new MozWebSocket("ws://120.78.162.85:4399/start");
                    }
                    catch (ex) {
                        WebSocketsExist = false;
                    }
                }
                if (WebSocketsExist) {
                    Log("您的浏览器支持WebSocket!", "OK");
                } else {
                    Log("您的浏览器不支持WebSocket。请选择其他的浏览器再尝试连接服务器。", "ERROR");
                    document.getElementById("ToggleConnection").disabled = true;
                }
            });
            ToggleConnectionClicked();
        });
    </script>
    </form>
</body>
</html>
