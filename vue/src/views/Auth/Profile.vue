<!-- 个人主页 -->
<template>
    <div class="profile-page">
        <!-- 左侧信息栏 -->
        <div class="sidebar">
            <!-- 用户名 -->
            <h2 class="username">{{ user.username }}</h2>
            <!-- 旅行状态 -->
            <div class="stats">
                <!-- 旅行过多少地方 -->
                <div>
                    <strong>{{totalPlaces}}</strong>
                    <p>places</p>
                </div>
                <!-- 累计打卡次数 -->
                <div>
                    <strong>{{ totalTimes }}</strong>
                    <p>times</p>
                </div>
            </div>
            <!-- 编辑资料 -->
            <button class="edit-button" @click="startEditing">编辑资料</button>
            <button class="logout-button" @click="showLogoutDialog">注销账号</button>
        </div>

        <!-- 右侧主展示区 -->
        <div class="main-area">
            <!-- 顶部标签 -->
            <div class="top">
                <div class="tabs">
                    <button :class="{ active: activeTab === 'map' }" @click="scrollTo('map')">Map</button>
                    <button :class="{ active: activeTab === 'timeline' }" @click="scrollTo('timeline')">Timeline</button>
                </div>

                <HoverButton text="返回首页" @click="handleBack" />
            </div>


            <!-- 内容区 -->
            <div class="content">
                <!-- 地图部分 -->
                <div ref="mapSection" class="section map-section">
                    <div class="header-row">
                        <h2>我的旅行地图</h2>
                    </div>
                    <div id="allmap"></div>
                </div>

                <div ref="timelineSection" class="section timeline-section">
                    <div class="header-row mb-6">
                        <h2 class="text-2xl font-bold">我的旅行时间线</h2>
                    </div>
                    <div class="w-full ">
                        <TimeLine :items="posts">
                            <template v-for="(item) in posts" :key="item.id" #[item.id]>
                                <div class="timeline-item flex flex-col md:flex-row items-start gap-4 p-20 rounded-lg shadow-md bg-white dark:bg-neutral-800 mb-6 transition hover:scale-[1.01] -ml-30 md:-ml-64 "
                                     @click="openEditor(item)">
                                    <!-- 时间 -->
                                    <div class="md:w-1/5 text-neutral-500 text-sm font-medium">
                                        <p>{{ formatDate(item.beginTime) }}</p>
                                        <p class="text-center">-</p>
                                        <p>{{ formatDate(item.endTime) }}</p>
                                    </div>

                                    <!-- 图片 -->
                                    <div class="md:w-1/4 w-full flex justify-center md:justify-start">
                                        <img v-if="item.imageUrls.length > 0"
                                             :src="getFullImageUrl(item.imageUrls[0].url)"
                                             class="w-32 h-32 object-cover rounded-lg border" />
                                    </div>

                                    <!-- 内容 -->
                                    <div class="md:w-3/5 w-full">
                                        <h3 class="text-lg font-semibold text-neutral-700 dark:text-neutral-200">
                                            {{ item.locationName }}
                                        </h3>
                                        <p class="mt-1 text-sm text-neutral-600 dark:text-neutral-400 whitespace-pre-line">
                                            {{ item.content }}
                                        </p>
                                    </div>
                                </div>

                            </template>
                        </TimeLine>
                    </div>
                </div>

            </div>
        </div>
    </div>

    <!-- 弹窗：编辑资料 -->
    <el-dialog v-model="isEditDialogVisible" title="编辑资料" width="400px">
        <el-form label-width="80px">
            <el-form-item label="用户名">
                <el-input v-model="editForm.username" placeholder="请输入用户名" />
            </el-form-item>
            <el-form-item label="密码">
                <el-input v-model="editForm.password" type="password" placeholder="请输入新的密码" />
            </el-form-item>
        </el-form>

        <template #footer>
            <el-button @click="isEditDialogVisible = false">取消</el-button>
            <el-button type="primary" @click="saveProfile">保存</el-button>
        </template>
    </el-dialog>

    <!-- 弹窗：确认注销 -->
    <el-dialog v-model="isLogoutDialogVisible" title="确认注销账号" width="400px">
        <p>您确定要注销账号吗？此操作不可逆！</p>
        <template #footer>
            <el-button @click="isLogoutDialogVisible = false">取消</el-button>
            <el-button type="primary" @click="handleCancel">确认注销</el-button>
        </template>
    </el-dialog>

    <!-- 弹窗：编辑旅行记录 -->
    <el-button @click="openEdit">编辑</el-button>
    <el-dialog v-model="editPostDialogVisible" title="编辑打卡内容" width="600px">
        <el-form label-position="top">
            <!-- 时间选择 -->
            <el-form-item label="编辑时间">
                <el-date-picker v-model="editingPost.beginTime"
                                type="date"
                                format="YYYY-MM-DD"
                                value-format="YYYY-MM-DD"
                                placeholder="选择旅行开始日期"
                                style="width: 100%; margin-bottom: 10px;" />
                <el-date-picker v-model="editingPost.endTime"
                                type="date"
                                format="YYYY-MM-DD"
                                value-format="YYYY-MM-DD"
                                placeholder="选择旅行结束日期"
                                style="width: 100%; margin-bottom: 10px;" />
            </el-form-item>

            <!-- 描述 -->
            <el-form-item label="描述">
                <el-input v-model="editingPost.content" type="textarea" :rows="4" placeholder="请输入打卡描述" />
            </el-form-item>

            <!-- 图片管理 -->
            <el-form-item label="图片预览">
                <draggable v-model="editingPost.imageUrls"
                           item-key="id"
                           animation="200"
                           ghost-class="drag-ghost"
                           handle=".drag-handle">
                    <template #item="{ element: img, index }">
                        <div class="image-item" :class="{ active: index === currentImageIndex }" @click="currentImageIndex = index">
                            <span class="drag-handle cursor-move text-xl mr-3 select-none">☰</span>
                            <el-image :src="getFullImageUrl(img.url)"
                                      :preview-src-list="[getFullImageUrl(img.url)]"
                                      fit="cover"
                                      class="thumbnail-img" />
                        </div>
                    </template>
                </draggable>
            </el-form-item>

            <el-form-item>
                <el-upload :auto-upload="false"
                           :show-file-list="false"
                           :before-upload="() => false"
                           :on-change="handleFileChange">
                    <el-button type="primary" icon="Plus">添加图片</el-button>
                </el-upload>
            </el-form-item>


            <el-form-item>
                <el-popconfirm title="确定删除当前图片？" @confirm="deleteCurrentImage">
                    <template #reference>
                        <el-button type="danger" icon="Delete" :disabled="editingPost.imageUrls.length <= 1">删除当前图片</el-button>
                    </template>
                </el-popconfirm>
            </el-form-item>
        </el-form>

        <template #footer>
            <div class="flex justify-between w-full">
                <el-button type="danger" @click="handleDelete" :loading="deleting">删除记录</el-button>
                <div class="space-x-2">
                    <el-button @click="editPostDialogVisible = false">取消</el-button>
                    <el-button type="primary" :loading="isSaving" @click="handleSave">保存</el-button>
                </div>
            </div>
        </template>
    </el-dialog>


</template>

<script setup>
    import HoverButton from '@/components/HoverButton.vue'
    import TimeLine from '@/components/TimeLine.vue'
    import draggable from "vuedraggable";
    import axios from 'axios'

    import { useRouter } from 'vue-router'
    import { ref, onMounted, onUnmounted } from 'vue'
    import { authAxios } from '@/utils/request'
    import { useAuthStore } from '@/stores/auth'
    import { ElMessage, ElMessageBox } from 'element-plus'
    import { ArrowUp, ArrowDown, Plus } from '@element-plus/icons-vue'
    import { Motion } from "motion-v";
    import { computed } from 'vue'
    import { Delete } from '@element-plus/icons-vue';


    // 计算位置数和次数
    const totalPlaces = computed(() => {
        const uniquePlaces = new Set(posts.value.map(post => post.locationName))
        return uniquePlaces.size
    })

    const totalTimes = computed(() => posts.value.length)
    // 地图实例变量
    let map = null;
    const loadBMapGL = () => {
        return new Promise((resolve, reject) => {
            // 如果已经加载，直接返回
            if (window.BMapGL) {
                resolve(window.BMapGL);
                return;
            }
            // 清理可能存在的旧脚本
            const existingScripts = document.querySelectorAll('script[src*="map.baidu.com"]');
            existingScripts.forEach(script => script.remove());
            // 创建新的脚本标签
            const script = document.createElement("script");
            script.type = "text/javascript";
            script.src = "https://api.map.baidu.com/api?type=webgl&v=1.0&ak=OUxgi9tGCKkijW4VW8d5F8FxcFRNfDfz";
            // 添加回调函数名（重要！）
            script.src += "&callback=initBMapGL";
            // 全局回调函数
            window.initBMapGL = () => {
                if (window.BMapGL) {
                    console.log("百度地图API已加载", window.BMapGL);
                    resolve(window.BMapGL);
                } else {
                    reject(new Error("BMapGL对象未定义"));
                }
                // 清理回调函数
                delete window.initBMapGL;
            };
            // 错误处理
            script.onerror = (error) => {
                console.error("脚本加载失败:", error);
                delete window.initBMapGL;
                reject(new Error("百度地图脚本加载失败"));
            };
            // 添加到文档头部
            document.head.appendChild(script);
        });
    };

    // 初始化地图控件
    const initMapControls = (map) => {
        // 添加缩放控件
        const zoomControl = new BMapGL.NavigationControl({
            anchor: window.BMAP_ANCHOR_TOP_LEFT,
            type: window.BMAP_NAVIGATION_CONTROL_ZOOM
        });
        map.addControl(zoomControl);
        // 启用键盘操作
        map.enableKeyboard();
    };

    // 当页面加载完成时，初始化地图
    onMounted(() => {
        loadBMapGL().then(BMapGL => {
            console.log("开始初始化地图...");

            // 创建地图实例
            map = new BMapGL.Map("allmap");

            // 设置中心点和缩放级别
            map.centerAndZoom(new BMapGL.Point(108.948024, 34.263161), 5);

            // 启用滚轮缩放
            map.enableScrollWheelZoom(true);

            // 初始化地图控件
            initMapControls(map);

            console.log("地图初始化完成");
        }).catch(error => {
            console.error("地图初始化失败：", error);
            // 这里可以添加用户友好的错误提示
        });
    });

    // 组件卸载时清理
    onUnmounted(() => {
        if (map) {
            map.destroy();
            map = null;
        }
    });

    // 编辑资料弹窗默认隐藏
    const isEditDialogVisible = ref(false)
    // 注销弹窗默认隐藏
    const isLogoutDialogVisible = ref(false);

    const editForm = ref({
        username: '',
        password: ''
    })

    const user = ref({
        id: '',
        username: '',
        password: '',
        checkins: []
    })

    const activeTab = ref('map')
    const mapSection = ref(null)
    const timelineSection = ref(null)
    const router = useRouter()
    const fileInput = ref(null)


    // 处理编辑资料按钮点击事件
    const startEditing = () => {
        editForm.value.username = user.value.username
        isEditDialogVisible.value = true
    }

    // 保存用户信息
    const saveProfile = async () => {
        try {
            const response = await authAxios.put('/user/info', {
                username: editForm.value.username,
                password: editForm.value.password
            })
            user.value.username = editForm.value.username
            user.value.password = editForm.value.password
            ElMessage.success('资料更新成功')
            isEditDialogVisible.value = false
        } catch (error) {
            ElMessage.error('更新失败，请重试')
        }
    }

    // 处理注销账号按钮点击事件
    const showLogoutDialog = () => {
        isLogoutDialogVisible.value = true;
    };

    // 注销操作
    const handleCancel = async () => {
        try {
            // 调用注销接口，执行注销逻辑
            await authAxios.delete('/user');
            console.log("注销成功");
            // 注销成功后，清除本地存储的token
            const authStore = useAuthStore()
            authStore.logout()

            // 注销成功后，跳转到首页
            router.push('/');

            // 关闭弹窗
            isLogoutDialogVisible.value = false;
        } catch (err) {
            console.error("注销失败:", err);
        }
    };

    const posts = ref([]);

    // 加载用户数据
    onMounted(async () => {
        const authStore = useAuthStore()
        console.log(authStore.token)
        try {
            const response = await authAxios.get('/user/me')
            console.log('拿到的用户数据', response.data)
            console.log(response.data.data)

            // 把后端返回的数据填充到 user
            user.value.username = response.data.data.username ?? '游客'
            user.value.password = response.data.data.password ?? ''
            user.value.id = response.data.data.id ?? ''

            console.log('加载用户数据', user)


            // 加载用户的游记
            await loadPosts()

        } catch (error) {
            console.error('获取个人信息失败：', error)
        }
    })

    const loadPosts = async () => {
        try {
            const response = await authAxios.get('/travel-posts/me');
            if (response.data.code === 200) {
                posts.value = response.data.data;
                posts.value.sort((a, b) => new Date(b.beginTime) - new Date(a.beginTime));
                addMarkers();
                console.log('posts', posts)
            } else {
                console.error('获取游记失败：', response.data.message);
            }
        } catch (err) {
            console.error('请求游记接口出错：', err);
        }
    }


    //处理函数url
    const getFullImageUrl = (url) => {
        const baseUrl = 'http://localhost:5008';
        return url?.startsWith('http') ? url : `${baseUrl}${url}`;
    };

    // 加载地图标记
    let currentMarkers = [];

    const addMarkers = () => {
        const BMapGL = window.BMapGL;
        if (!map || !posts.value) return;

        // 清除地图上的旧标记
        currentMarkers.forEach(marker => map.removeOverlay(marker));
        currentMarkers = [];

        // 添加新标记
        posts.value.forEach(item => {
            const point = new BMapGL.Point(item.longitude, item.latitude);
            const marker = new BMapGL.Marker(point);
            map.addOverlay(marker);
            currentMarkers.push(marker); // 添加到 marker 列表中

            // 创建信息窗口内容，带图片和地点名
            const imageUrl = getFullImageUrl(item.imageUrls[0].url);
            const content = `
<div style="width:220px; height:250px; border: 1px solid #666; border-radius: 8px; background: white; padding: 8px; position: relative; font-family: Arial, sans-serif;">
  <div style="font-weight: bold; font-size: 16px; margin-bottom: 8px;">${item.locationName}</div>
  <img src="${imageUrl}" style="width: 200px; height: 200px; object-fit: cover; border-radius: 6px;" />
</div>
`;

            const infoWindow = new BMapGL.InfoWindow(content, {
                offset: new BMapGL.Size(0, -30)
            });

            marker.addEventListener('click', () => {
                map.openInfoWindow(infoWindow, point);
            });
        });
    };

    const scrollTo = (section) => {
        if (section === 'map' && mapSection.value) {
            mapSection.value.scrollIntoView({ behavior: 'smooth' })
        }
        if (section === 'timeline' && timelineSection.value) {
            timelineSection.value.scrollIntoView({ behavior: 'smooth' })
        }
    }

    // 根据滚动位置，动态更新 activeTab
    const handleScroll = () => {
        const mapTop = mapSection.value?.getBoundingClientRect().top || 0
        const timelineTop = timelineSection.value?.getBoundingClientRect().top || 0

        if (timelineTop < window.innerHeight / 2) {
            activeTab.value = 'timeline'
        } else {
            activeTab.value = 'map'
        }
    }

    onMounted(() => {
        window.addEventListener('scroll', handleScroll)
    })

    onUnmounted(() => {
        window.removeEventListener('scroll', handleScroll)
    })

    // 处理返回按钮点击事件
    const handleBack = () => {
        router.push('/')
    }

    // 格式化日期的函数
    const formatDate = (dateString) => {
        const date = new Date(dateString);
        const year = date.getFullYear();
        const month = date.getMonth() + 1; // 月份从0开始，+1是为了正确显示
        const day = date.getDate();
        return `${year}.${month}.${day}`; // 拼接成 'YYYY.MM.DD' 格式
    };

    // 点击卡片编辑卡片信息
    const editPostDialogVisible = ref(false)
    const editingPost = ref({
        postId: null,
        content: '',
        locationName: '',
        latitude: 0,
        longitude: 0,
        beginTime: '',
        endTime: '',
        imageUrls: [],
    })

    const currentImageIndex = ref(0);
    const isSaving = ref(false);
    const newFiles = ref([]);

    const openEditor = async (item) => {
        editingPost.value = {
            postId: item.id,  
            imageUrls: item.imageUrls ?? [],
            content: item.content,
            locationName: item.locationName,
            latitude: item.latitude,
            longitude: item.longitude,
            beginTime: item.beginTime,
            endTime: item.endTime
        };
        currentImageIndex.value = 0;
        editPostDialogVisible.value = true;
    };

    function handleBeforeUpload(file) {
        newFiles.value.push(file);
        return false;
    }

    const authHeader = {
        headers: {
            'Authorization': 'Bearer ' + localStorage.getItem('token')
        }
    };

    async function handleFileChange(file) {
        const postId = editingPost.value.postId;
        const formData = new FormData();
        formData.append('file', file.raw);

        try {
            const res = await axios.post(`/api/travel-posts/${postId}/images`, formData, authHeader);
            // 添加上传成功的图片到图片数组
            console.log('上传图片返回:', res.data);
            editingPost.value.imageUrls.push(res.data);
        } catch (error) {
            console.error('上传失败:', error);
        }
    }

    async function deleteCurrentImage() {
        const img = editingPost.value.imageUrls[currentImageIndex.value];
        console.log('当前图片列表:', editingPost.value.imageUrls);
        try {
            await axios.delete(`/api/travel-posts/images/${img.id}`, authHeader);
            editingPost.value.imageUrls.splice(currentImageIndex.value, 1);
            currentImageIndex.value = 0;
        } catch (error) {
            console.error('删除图片失败:', error.response || error);
        }
    }


    async function handleSave() {
        isSaving.value = true;
        const post = editingPost.value;

        console.log(post.content, post.locationName, post.beginTime, post.endTime);


        await axios.patch(
            `/api/travel-posts/${post.postId}/text`,
            {
                content: post.content,
                locationName: post.locationName,
                beginTime: post.beginTime,
                endTime: post.endTime
            },
            authHeader
        );

        const sortedImages = post.imageUrls.map((img, index) => ({
            id: img.id,
            order: index
        }));
        await axios.patch(
            `/api/travel-posts/${post.postId}/images/order`,
            sortedImages,
            authHeader
        );

        // 保存成功后，同步更新 posts 数组里的对应项
        const index = posts.value.findIndex(p => p.id === post.postId);
        if (index !== -1) {
            posts.value[index] = {
                ...posts.value[index],
                content: post.content,
                locationName: post.locationName,
                beginTime: post.beginTime,
                endTime: post.endTime,
                imageUrls: post.imageUrls, // 如果图片也有更新
            };
        }
        await addMarkers();

        ElMessage.success('保存成功');
        isSaving.value = false;
        editPostDialogVisible.value = false;
    }


        // 上一张图片
        function prevImage() {
            if (currentImageIndex.value > 0) {
                currentImageIndex.value--
            }

        }

        // 下一张图片
        function nextImage() {
            if (currentImageIndex.value < editingPost.value.imageUrls.length - 1) {
                currentImageIndex.value++
            }

        }



       

        const deleting = ref(false)
        // 删除记录
        const handleDelete = async () => {
            try {
                await ElMessageBox.confirm(
                    '确定要删除该游记记录吗？删除后将无法恢复。',
                    '删除确认',
                    {
                        confirmButtonText: '删除',
                        cancelButtonText: '取消',
                        type: 'warning',
                    }
                );
                console.log('即将删除的 postId:', editingPost.value.postId);

                deleting.value = true;
                await authAxios.delete(`/travel-posts/${editingPost.value.postId}`);

                ElMessage.success('删除成功');
                editPostDialogVisible.value = false;

                await loadPosts();
                console.log(posts);
            } catch (error) {
                if (error !== 'cancel') {
                    ElMessage.error('删除失败');
                    console.error('删除失败：', error);
                }
            } finally {
                deleting.value = false;
            }
        };
    
</script>


<style scoped>
    .profile-page {
        display: flex;
        min-height: 100vh;
        background-image: url("/assets/背景3.jpeg");
        background-size: cover;
        background-position: center;
        z-index: -1;
    }

    /* 左侧 */
    .sidebar {
        width: 260px;
        background-color: #ffffffa0;
        padding: 24px;
        box-shadow: 2px 0 8px rgba(0, 0, 0, 0.1);
        position: sticky;
        top: 0;
        height: 100vh;
    }

    /* 头像 */
    .avatar {
        width: 120px;
        height: 120px;
        background-color: #ddd;
        border-radius: 50%;
        margin: 0 auto 16px;
    }

    /* 用户名 */
    .username {
        text-align: center;
        margin: 8px 0;
        font-size: 20px;
    }

    /* 个性签名 */
    .bio {
        text-align: center;
        font-size: 14px;
        color: #777;
    }

    .stats {
        display: flex;
        justify-content: space-around;
        margin: 20px 0;
    }

        .stats div {
            text-align: center;
        }

    .edit-button, .logout-button {
        display: block;
        width: 100%;
        padding: 8px;
        background-color: #4f46e5;
        color: white;
        border: none;
        border-radius: 6px;
        cursor: pointer;
        margin-top: 10px;
    }

        .logout-button:hover,
        .edit-button:hover {
            background-color: #4338ca;
        }

    /* 右侧主区域 */
    .main-area {
        flex: 1;
        padding: 24px;
    }

    .top {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 16px;
        position: sticky;
        top: 0;
        z-index: 100;
    }

    .tabs {
        display: flex;
        gap: 16px;
        margin-bottom: 24px;
    }

        .tabs button {
            background: none;
            border: none;
            font-size: 16px;
            padding: 8px 16px;
            cursor: pointer;
            color: #666;
            border-bottom: 2px solid transparent;
        }

            .tabs button.active {
                color: #333;
                border-color: #4f46e5;
            }

    .section {
        background: white;
        margin-bottom: 60px;
        padding: 40px;
        border-radius: 8px;
        min-height: 600px;
    }

    .map-section {
        display: flex;
        flex-direction: column;
        height: 100%; /* 父容器高度 */
    }

    #allmap {
        width: 100%; /* 确保地图区域宽度填满父容器 */
        height: 400px; /* 设置地图区域的固定高度 */
    }

    .header-row {
        display: flex;
        justify-content: center;
        align-items: center;
        margin-bottom: 16px; /* 增加与地图的间距 */
    }

    h2 {
        font-size: 20px;
        text-align: center;
        font-weight: bold;
    }

    /* 去除水印 */
    ::v-deep(.BMapGL_cpyCtrl) {
        display: none;
    }

    ::v-deep(.anchorBL) {
        display: none !important;
    }

    .el-input {
        border-radius: 8px;
    }

    .image-item-wrapper {
        margin-bottom: 10px;
        width: 100%;
    }

    .image-item {
        display: flex;
        align-items: center;
        background-color: #f9f9f9;
        border: 2px solid transparent;
        border-radius: 10px;
        padding: 10px;
        transition: border 0.2s;
    }

        .image-item.active {
            border-color: #409EFF;
        }

    .thumbnail-img {
        width: 80px;
        height: 80px;
        border-radius: 8px;
        object-fit: cover;
        margin-right: 15px;
    }

    .image-info {
        flex: 1;
    }

    .image-title {
        font-weight: 600;
        font-size: 16px;
        margin-bottom: 4px;
    }

    .image-meta {
        font-size: 12px;
        color: #666;
    }

    .el-upload {
        width: 100px;
    }
</style>