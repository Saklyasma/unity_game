import { Route, Routes, useLocation } from 'react-router-dom'
import Navbar from './components/Navbar.jsx'
import Footer from './components/Footer.jsx'
import ProtectedRoute from './components/ProtectedRoute.jsx'
import AdminLayout from './components/AdminLayout.jsx'
import Chatbot from './components/Chatbot.jsx'
import Home from './pages/Home.jsx'
import SignUp from './pages/SignUp.jsx'
import SignIn from './pages/SignIn.jsx'
import NotFound from './pages/NotFound.jsx'
import AdminDashboard from './pages/admin/AdminDashboard.jsx'
import ManageQuizzes from './pages/admin/ManageQuizzes.jsx'
import ManageGameLevels from './pages/admin/ManageGameLevels.jsx'
import ConfigurePowerUps from './pages/admin/ConfigurePowerUps.jsx'
import PlayerStatistics from './pages/admin/PlayerStatistics.jsx'
import ManageUserAccounts from './pages/admin/ManageUserAccounts.jsx'
import EducationalContent from './pages/admin/EducationalContent.jsx'
import AdminProfile from './pages/admin/AdminProfile.jsx'
import ManageCountries from './pages/admin/ManageCountries.jsx'
import ManageBotStats from './pages/admin/ManageBotStats.jsx'
import ManageTeams from './pages/admin/ManageTeams.jsx'
import ManageMatches from './pages/admin/ManageMatches.jsx'
import ManagePredictions from './pages/admin/ManagePredictions.jsx'

export default function App() {
  const location = useLocation()
  const isAdminRoute = location.pathname.startsWith('/admin')

  return (
    <>
      {!isAdminRoute && <Navbar />}

      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/sign-up" element={<SignUp />} />
        <Route path="/sign-in" element={<SignIn />} />

        <Route
          path="/admin"
          element={
            <ProtectedRoute>
              <AdminLayout />
            </ProtectedRoute>
          }
        >
          <Route index element={<AdminDashboard />} />
          <Route path="countries" element={<ManageCountries />} />
          <Route path="quizzes" element={<ManageQuizzes />} />
          <Route path="bot-stats" element={<ManageBotStats />} />
          <Route path="teams" element={<ManageTeams />} />
          <Route path="matches" element={<ManageMatches />} />
          <Route path="predictions" element={<ManagePredictions />} />
          <Route path="game-levels" element={<ManageGameLevels />} />
          <Route path="power-ups" element={<ConfigurePowerUps />} />
          <Route path="statistics" element={<PlayerStatistics />} />
          <Route path="users" element={<ManageUserAccounts />} />
          <Route path="content" element={<EducationalContent />} />
          <Route path="profile" element={<AdminProfile />} />
        </Route>

        <Route path="*" element={<NotFound />} />
      </Routes>

      {!isAdminRoute && <Footer />}
      <Chatbot />
    </>
  )
}
