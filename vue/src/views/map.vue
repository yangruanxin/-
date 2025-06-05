<!-- vue3引入百度api -->
<template>
  <div class="map-container">
    <div class="header-row">
      <el-button @click="resetMap" class="reset-map">重置地图</el-button> 
      <h2>点击地图，选择你的旅行目的地</h2>
      <HoverButton text="返回" @click="handleBack" class="back"/>
    </div>
    <div id="allmap"></div>
  </div>

  <!-- 上传旅行记录浮窗 -->
  <div v-if="showModal" class="modal-overlay">
    <div class="modal">
      <h3 
        class="relative z-20 font-sans text-base font-bold dark:text-neutral-300"
      >添加旅行记录</h3>
      <p>目的地：{{ locationInfo.name }}</p>
      <div class="upload">
        <FileSubmit class="additional-class" @onChange="handleImageUpload"></FileSubmit>
        <div style="flex: 1;">
          <div class="travel-time">
            <el-date-picker
              v-model="travelStart"
              type="date"
              format="YYYY-MM-DD"
              value-format="YYYY-MM-DD"
              placeholder="选择旅行开始日期"
              style="width: 100%; margin-bottom: 10px;"
            />
            <el-date-picker
              v-model="travelEnd"
              type="date"
              format="YYYY-MM-DD"
              value-format="YYYY-MM-DD"
              placeholder="选择旅行结束日期"
              style="width: 100%; margin-bottom: 10px;"
            />
          </div>

          <!-- 描述文本框 -->
          <textarea v-model="description" placeholder="记录一下你的感受吧..." rows="4"></textarea>
      </div>
      </div>


      <div class="buttons">
        <el-button type="primary" @click="submit">提交</el-button>
        <el-button type="primary" @click="closeModal">取消</el-button>
      </div>
    </div>
  </div>

</template>

<script setup>
import HoverButton from "@/components/HoverButton.vue";
import FileSubmit from "@/components/FileSubmit.vue";
import {useRouter} from "vue-router";
import { ref,onMounted } from "vue";
import { authAxios } from '@/utils/request'
import { useAuthStore } from '@/stores/auth'

const router=useRouter();

const showModal = ref(false) // 控制浮窗显示
const locationInfo = ref({}) // 保存选中的地点信息（名字、经纬度）
const uploadImage = ref([]) // 保存上传的图片
const description = ref('') // 保存填写的描述
const travelStart = ref('') // 保存旅行开始日期
const travelEnd = ref('') // 保存旅行结束日期
let map=null

//处理返回首页按钮点击事件
const handleBack = () => {
  router.push('/')
}

//为什么说我没修改这个文件啊啊啊

// 动态加载百度地图脚本，返回 Promise
const loadBMapGL = () => {
  return new Promise((resolve, reject) => {
    if (window.BMapGL) {
      resolve(window.BMapGL);
      return;
    }

    // 清理旧脚本
    const existingScripts = document.querySelectorAll('script[src*="map.baidu.com"]');
    existingScripts.forEach(script => script.remove());

    // 创建脚本标签
    const script = document.createElement("script");
    script.type = "text/javascript";
    script.src = "https://api.map.baidu.com/api?type=webgl&v=1.0&ak=OUxgi9tGCKkijW4VW8d5F8FxcFRNfDfz&callback=initBMapGL";

    // 设置全局回调
    window.initBMapGL = () => {
      if (window.BMapGL) {
        resolve(window.BMapGL);
      } else {
        reject(new Error("BMapGL 未定义"));
      }
      delete window.initBMapGL; // 清理回调
    };

    // 错误处理
    script.onerror = () => {
      delete window.initBMapGL;
      reject(new Error("百度地图脚本加载失败"));
    };

    document.head.appendChild(script);
  });
};

// 初始化地图
const initMap = (BMapGL) => {
  map = new BMapGL.Map("allmap");
  map.centerAndZoom(new BMapGL.Point(108.948024, 34.263161), 5);
  map.enableScrollWheelZoom(true);

  console.log("地图初始化完成，准备绑定点击事件");

  map.addEventListener("click", (e) => {
    console.log("点击了地图", e.point);
    const lng = e.latlng.lng;
      const lat = e.latlng.lat;
      const authStore = useAuthStore()
      console.log('当前 token:', authStore.token)

      fetch(`https://121.43.136.251:8080/api/map/reverse-geocode?lat=${lat}&lng=${lng}`, {
          method: 'GET',
          headers: {
              'Authorization': `Bearer ${authStore.token}`,
              'Content-Type': 'application/json'
          }
      })
      .then(res => res.text())
      .then(text => {
        console.log("API响应文本：", text);
        try {
          const data = JSON.parse(text);
          if (data.status === 0) {
            const address = data.result.addressComponent;
            let name = '';

            if (address.country === '中国') {
              name = `${address.province} ${address.city}`;
            } else {
              name = `${address.country} ${address.city || ''}`.trim();
            }

            locationInfo.value = {
              name,
              lng,
              lat
            };
            showModal.value = true;
          } else {
            alert('逆地理编码失败');
          }
        } catch (e) {
          console.error("解析 JSON 失败", e);
        }
      })
      .catch(err => {
        console.error("请求失败", err);
        alert('请求失败');
      });
  });
};

// Vue 生命周期钩子
onMounted(() => {
  console.log("DOM 渲染完成，准备加载地图");

  loadBMapGL()
    .then(BMapGL => {
      console.log("百度地图脚本加载成功，开始初始化地图");
      initMap(BMapGL);
    })
    .catch(err => {
      console.error("地图加载失败：", err);
      alert("地图加载失败，请稍后重试");
    });
});

const handleImageUpload = (files) => {
  if (files.length > 0) {
    uploadImage.value = files;
  }
}

const formatDateTime = (dateStr) => {
  return dateStr ? `${dateStr} 00:00:00` : '';
}

const submit = async () => {

    // 校验必填字段
  if (!uploadImage.value || uploadImage.value.length === 0) {
    alert("请上传至少一张照片！");
    return;
  }

  if (!travelStart.value || !travelEnd.value) {
    alert("请选择旅行起止时间！");
    return;
  }

  if (!description.value || description.value.trim() === "") {
    alert("请填写描述内容！");
    return;
  }

  const formData = new FormData();
  console.log("LocationInfo.value",locationInfo.value)
  formData.append("LocationName", locationInfo.value.name);
  formData.append("Longitude", locationInfo.value.lng);
  formData.append("Latitude", locationInfo.value.lat);
  formData.append("Content", description.value);
  formData.append("BeginTime", formatDateTime(travelStart.value));
  formData.append("EndTime", formatDateTime(travelEnd.value));

  if (uploadImage.value && uploadImage.value.length > 0) {
    uploadImage.value.forEach((file,index) => {
      formData.append("ImageUrls", file);
      formData.append("Orders", index.toString()); 
      console.log(index)
    });
  }

  try {
    const response = await authAxios.post("/travel-posts", formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    });
    console.log("提交成功：", response.data);
    alert("提交成功！");
    closeModal();
  } catch (error) {
    console.error("提交失败：", error);
    alert(`提交失败：${error.response?.data?.message || error.message}`);
  }
};

const closeModal = () => {
  showModal.value = false
  locationInfo.value = {}
  uploadImage.value = null
  description.value = ''
}

//重置地图
const resetMap = () => {
  let BMapGL = window.BMapGL;
  // 重置为初始的中心点和缩放级别
  map.centerAndZoom(new BMapGL.Point(108.948024, 34.263161), 5); 
  map.clearOverlays(); // 可选：清除任何已添加的标记或覆盖物
};

</script>

<style scoped>
/* 去除水印 */
::v-deep(.BMapGL_cpyCtrl) {
  display: none;
}
 
::v-deep(.anchorBL) {
  display: none !important;
}
 
.map-container {
  display: flex;
  flex-direction: column;
  height: 100vh; /* 父容器明确高度 */
}

.header-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 4px; /* 添加与地图的间距 */
}

h2 {
  padding: 16px;
  text-align: center;
  font-weight: bold;
  font-size: 20px;
  margin: 0; /* 去除默认margin */
}

#allmap {
  flex: 1; /* 占据剩余空间 */
  width: 100%;
  position: relative;
  z-index: 10;
}

.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: rgba(0, 0, 0, 0.5); /* 半透明背景 */
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000; /* 必须高过地图 */
}

.modal {
  background: #fff;
  padding: 20px;
  border-radius: 8px;
  z-index: 1001; /* 更高，确保弹窗内容在最上面 */
  width: 800px; /* 增加弹窗宽度 */
  max-width: 80%; /* 确保在小屏幕下不超出 */
}

.reset-map {
  padding: 8px 16px;
  background-color: #007bff;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  margin-left: 10px;
}

.reset-map:hover {
  background-color: #0056b3;
}

.back{
  margin-right: 10px;
}

.upload {
  display: flex;
  gap: 20px; /* 控制上传图片和文字之间的间距 */
  align-items: stretch; /* 顶对齐 */
  margin-bottom: 20px; /* 和下面的提交按钮拉开点距离 */
}

.upload textarea {
  flex: 1; /* 让textarea尽量占满剩余空间 */
  resize: vertical; /* 允许用户上下拉伸文本框 */
}

.addtional-class{
  flex:1;
  width: 400px;
  resize:vertical;
}
</style>